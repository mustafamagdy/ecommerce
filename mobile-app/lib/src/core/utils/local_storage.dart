import 'dart:convert';

import 'package:admin_desktop/src/models/data/tenant_data.dart';
import 'package:shared_preferences/shared_preferences.dart';

import '../../models/models.dart';
import '../constants/constants.dart';

class LocalStorage {
  static SharedPreferences? _preferences;

  LocalStorage._();

  static LocalStorage? _localStorage;

  static LocalStorage instance = LocalStorage._();

  static Future<LocalStorage> getInstance() async {
    if (_localStorage == null) {
      _localStorage = LocalStorage._();
      await _localStorage!._init();
    }
    return _localStorage!;
  }

  static Future<SharedPreferences> getSharedPreferences() async {
    if (_preferences == null) {
      _localStorage = LocalStorage._();
      await _localStorage!._init();
    }
    return _preferences!;
  }

  Future<void> _init() async {
    _preferences = await SharedPreferences.getInstance();
  }

  Future<void> setToken(String? token) async {
    if (_preferences != null) {
      await _preferences!.setString(AppConstants.keyToken, token ?? '');
    }
  }

  String getToken() => _preferences?.getString(AppConstants.keyToken) ?? '';

  void deleteToken() => _preferences?.remove(AppConstants.keyToken);

  setPinCode(String pinCode) async {
    if (_preferences != null) {
      await _preferences!.setString(AppConstants.pinCode, pinCode);
    }
  }
  String getPinCode() => _preferences?.getString(AppConstants.pinCode) ?? '';

  void deletePinCode() => _preferences?.remove(AppConstants.pinCode);

  Future<void> setSettingsList(List<SettingsData> settings) async {
    if (_preferences != null) {
      final List<String> strings =
          settings.map((setting) => jsonEncode(setting.toJson())).toList();
      await _preferences!
          .setStringList(AppConstants.keyGlobalSettings, strings);
    }
  }

  List<SettingsData> getSettingsList() {
    final List<String> settings =
        _preferences?.getStringList(AppConstants.keyGlobalSettings) ?? [];
    final List<SettingsData> settingsList = settings
        .map(
          (setting) => SettingsData.fromJson(jsonDecode(setting)),
        )
        .toList();
    return settingsList;
  }

  void deleteSettingsList() =>
      _preferences?.remove(AppConstants.keyGlobalSettings);

  Future<void> setActiveLocale(String? locale) async {
    if (_preferences != null) {
      await _preferences!.setString(AppConstants.keyActiveLocale, locale ?? '');
    }
  }

  String getActiveLocale() =>
      _preferences?.getString(AppConstants.keyActiveLocale) ?? 'en';

  void deleteActiveLocale() =>
      _preferences?.remove(AppConstants.keyActiveLocale);

  Future<void> setTranslations(Map<String, dynamic>? translations) async {
    if (_preferences != null) {
      final String encoded = jsonEncode(translations);
      await _preferences!.setString(AppConstants.keyTranslations, encoded);
    }
  }

  Map<String, dynamic> getTranslations() {
    final String encoded =
        _preferences?.getString(AppConstants.keyTranslations) ?? '';
    if (encoded.isEmpty) {
      return {};
    }
    final Map<String, dynamic> decoded = jsonDecode(encoded);
    return decoded;
  }

  void deleteTranslations() =>
      _preferences?.remove(AppConstants.keyTranslations);

  Future<void> setSelectedCurrency(CurrencyData currency) async {
    if (_preferences != null) {
      final String currencyString = jsonEncode(currency.toJson());
      await _preferences!
          .setString(AppConstants.keySelectedCurrency, currencyString);
    }
  }

  CurrencyData getSelectedCurrency() {
    final map = jsonDecode(
        _preferences?.getString(AppConstants.keySelectedCurrency) ?? '');
    return CurrencyData.fromJson(map);
  }

  void deleteSelectedCurrency() =>
      _preferences?.remove(AppConstants.keySelectedCurrency);

  Future<void> setBags(List<BagData> bags) async {
    if (_preferences != null) {
      final List<String> strings =
          bags.map((bag) => jsonEncode(bag.toJson())).toList();
      await _preferences!.setStringList(AppConstants.keyBags, strings);
    }
  }

  List<BagData> getBags() {
    final List<String> bags =
        _preferences?.getStringList(AppConstants.keyBags) ?? [];
    final List<BagData> localBags = bags
        .map(
          (bag) => BagData.fromJson(jsonDecode(bag)),
        )
        .toList(growable: true);
    return localBags;
  }

  void deleteCartProducts() => _preferences?.remove(AppConstants.keyBags);

  Future<void> setUser(UserData? user) async {
    if (_preferences != null) {
      final String userString = user != null ? jsonEncode(user.toJson()) : '';
      await _preferences!.setString(AppConstants.keyUser, userString);
    }
  }

  UserData? getUser() {
    final savedString = _preferences?.getString(AppConstants.keyUser);
    if (savedString == null) {
      return null;
    }
    final map = jsonDecode(savedString);
    if (map == null) {
      return null;
    }
    return UserData.fromJson(map);
  }

  TenantData? getTenant() {
    final savedString = _preferences?.getString(AppConstants.keyTenant);
    if (savedString == null) {
      return null;
    }
    final map = jsonDecode(savedString);
    if (map == null) {
      return null;
    }
    return TenantData.fromJson(map);
  }

  void deleteUser() => _preferences?.remove(AppConstants.keyUser);

  void clearStore() {
    deletePinCode();
    deleteToken();
   deleteUser();
   deleteCartProducts();
  }
}
