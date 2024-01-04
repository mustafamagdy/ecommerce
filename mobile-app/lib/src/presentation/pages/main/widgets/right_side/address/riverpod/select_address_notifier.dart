import 'dart:async';

import 'package:admin_desktop/src/core/constants/constants.dart';
import 'package:admin_desktop/src/core/utils/app_connectivity.dart';
import 'package:admin_desktop/src/core/utils/app_helpers.dart';
import 'package:admin_desktop/src/core/utils/local_storage.dart';
import 'package:admin_desktop/src/repository/users_repository.dart';
import 'package:flutter/widgets.dart';
import 'package:geolocator/geolocator.dart';
import 'package:osm_nominatim/osm_nominatim.dart';
import 'package:location/location.dart' as location;
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_maps_flutter/google_maps_flutter.dart';

import '../../../../../../../models/data/location_data.dart';
import 'select_address_state.dart';

class SelectAddressNotifier extends StateNotifier<SelectAddressState> {
  final UsersRepository _usersRepository;
  Timer? _timer;
  final GeolocatorPlatform _geolocatorPlatform = GeolocatorPlatform.instance;

  SelectAddressNotifier(this._usersRepository)
      : super(SelectAddressState(textController: TextEditingController()));

  void setQuery(BuildContext context) {
    if (state.textController?.text.trim().isNotEmpty ?? false) {
      if (_timer?.isActive ?? false) {
        _timer?.cancel();
      }
      _timer = Timer(
        const Duration(milliseconds: 500),
        () {
          searchLocations();
        },
      );
    }
  }

  Future<void> searchLocations() async {
    state = state.copyWith(isSearching: true, isSearchLoading: true);
    try {
      final result = await Nominatim.searchByName(
        query: state.textController?.text.trim() ?? '',
        limit: 5,
        addressDetails: true,
        extraTags: true,
        nameDetails: true,
      );
      state = state.copyWith(searchedPlaces: result, isSearchLoading: false);
    } catch (e) {
      debugPrint('===> search location error $e');
      state = state.copyWith(isSearchLoading: false);
    }
  }

  void clearSearchField() {
    state.textController?.clear();
    state = state.copyWith(searchedPlaces: [], isSearching: false);
  }

  void setMapController(GoogleMapController controller) {
    state = state.copyWith(mapController: controller);
  }

  void setChoosing(bool value) {
    state = state.copyWith(isChoosing: value, isSearching: false);
  }

  void goToLocation({required Place place}) {
    state = state.copyWith(isSearching: false);
    state.textController?.text = place.displayName;
    state.mapController?.animateCamera(
      CameraUpdate.newCameraPosition(
        CameraPosition(
          bearing: 0,
          target: LatLng(place.lat, place.lon),
          tilt: 0,
          zoom: 17,
        ),
      ),
    );
    state = state.copyWith(
      location: LocationData(latitude: place.lat, longitude: place.lon),
    );
  }

  Future<void> gotToPlace(LocationData? location) async {
    state = state.copyWith(searchedPlaces: [], isSearching: false);
    Place? place;
    try {
      place = await Nominatim.reverseSearch(
        lat: location?.latitude,
        lon: location?.longitude,
        addressDetails: true,
        extraTags: true,
        nameDetails: true,
      );
      state.textController?.text = place.displayName;
    } catch (e) {
      debugPrint('===> go to my location error: $e');
      state.textController?.text = '';
    }
    if (place != null) {
      state.mapController?.animateCamera(
        CameraUpdate.newCameraPosition(
          CameraPosition(
            bearing: 0,
            target: LatLng(place.lat, place.lon),
            tilt: 0,
            zoom: 17,
          ),
        ),
      );
      state = state.copyWith(
        location: LocationData(latitude: place.lat, longitude: place.lon),
      );
    }
  }

  Future<void> goToMyLocation() async {
    var check = await _geolocatorPlatform.checkPermission();
    dynamic  latLng;
    if (check == LocationPermission.denied ||
        check == LocationPermission.deniedForever) {
      check = await Geolocator.requestPermission();
      if (check != LocationPermission.denied &&
          check != LocationPermission.deniedForever) {
        var loc = await Geolocator.getCurrentPosition();
        latLng = LatLng(loc.latitude, loc.longitude);
        state.mapController!
            .animateCamera(CameraUpdate.newLatLngZoom(latLng, 15));
      }
    } else {
      if (check != LocationPermission.deniedForever) {
        var loc = await Geolocator.getCurrentPosition();
          latLng = LatLng(loc.latitude, loc.longitude);
        state.mapController!
            .animateCamera(CameraUpdate.newLatLngZoom(latLng, 15));
      }
    }
    state = state.copyWith(searchedPlaces: [], isSearching: false);
    Place? place;
    try {
      place = await Nominatim.reverseSearch(
        lat: latLng.latitude,
        lon: latLng.longitude,
        addressDetails: true,
        extraTags: true,
        nameDetails: true,
      );
      state.textController?.text = place.displayName;
    } catch (e) {
      debugPrint('===> go to my location error: $e');
      state.textController?.text = '';
    }
    if (place != null) {
      state.mapController?.animateCamera(
        CameraUpdate.newCameraPosition(
          CameraPosition(
            bearing: 0,
            target: LatLng(place.lat, place.lon),
            tilt: 0,
            zoom: 17,
          ),
        ),
      );
      state = state.copyWith(
        location: LocationData(latitude: place.lat, longitude: place.lon),
      );
    }
  }

  Future<void> findMyLocation() async {
    location.LocationData? myLocationData;
    final locationInstance = location.Location();
    bool serviceEnabled;
    location.PermissionStatus permissionGranted;
    serviceEnabled = await locationInstance.serviceEnabled();
    if (!serviceEnabled) {
      serviceEnabled = await locationInstance.requestService();
      if (!serviceEnabled) {
        return;
      }
    }
    permissionGranted = await locationInstance.hasPermission();
    if (permissionGranted == location.PermissionStatus.denied) {
      permissionGranted = await locationInstance.requestPermission();
      if (permissionGranted != location.PermissionStatus.granted) {
        return;
      }
    }
    try {
      myLocationData = await locationInstance.getLocation();
    } catch (e) {
      myLocationData = null;
    }
    if (myLocationData != null) {

    }
  }

  Future<void> saveLocalAddress(
    bool? hasBack, {
    VoidCallback? onBack,
    VoidCallback? onGoMain,
  }) async {
    clearSearchField();
    state.mapController?.dispose();
  }

  Future<void> fetchLocationName(LatLng? latLng) async {
    state = state.copyWith(
      location: LocationData(
        latitude: latLng?.latitude,
        longitude: latLng?.longitude,
      ),
    );
    Place? place;
    try {
      place = await Nominatim.reverseSearch(
        lat: latLng?.latitude,
        lon: latLng?.longitude,
        addressDetails: true,
        extraTags: true,
        nameDetails: true,
      );
      state.textController?.text = place.displayName;
    } catch (e) {
      state.textController?.text = '';
    }
  }

  Future<void> checkDriverZone(
      {required BuildContext context, required LatLng? location}) async {
    final connected = await AppConnectivity.connectivity();
    if (connected) {
      state = state.copyWith(isLoading: true, isActive: false);
      final response = await _usersRepository.checkDriverZone(
          location ?? const LatLng(0.0, 0.0),
          LocalStorage.instance.getUser()?.shop?.id);
      response.when(
        success: (data) async {
          state = state.copyWith(isLoading: false, isActive: data);
          if (!data) {
            AppHelpers.showSnackBar(
              context,
              AppHelpers.getTranslation(TrKeys.noDriverZone),
            );
          }
        },
        failure: (activeFailure) {
          state = state.copyWith(isLoading: false);
        },
      );
    } else {
      if (mounted) {
        AppHelpers.showSnackBar(
            context, AppHelpers.getTranslation(TrKeys.noInternetConnection));
      }
    }
  }
}
