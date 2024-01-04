import 'package:admin_desktop/src/models/data/order_data.dart';
import 'package:admin_desktop/src/presentation/components/buttons/invoice_download.dart';
import 'package:flutter/material.dart';
import 'package:flutter_remix/flutter_remix.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:google_maps_flutter/google_maps_flutter.dart';
import 'package:intl/intl.dart';
import '../../../../../../core/constants/constants.dart';
import '../../../../../../core/utils/utils.dart';
import '../../../../../theme/theme.dart';
import '../../order_detail/widgets/order_status.dart';
import '../order_table_riverpod/order_table_provider.dart';
import 'map_buttons.dart';

class MapDialog extends ConsumerStatefulWidget {
  final OrderData orderData;

  const MapDialog({Key? key, required this.orderData}) : super(key: key);

  @override
  ConsumerState<MapDialog> createState() => _MapDialogState();
}

class _MapDialogState extends ConsumerState<MapDialog> {
  late GoogleMapController mapController;
  late LatLng _target;

  @override
  void initState() {
    WidgetsBinding.instance.addPostFrameCallback((timeStamp) {
      ref.read(orderTableProvider.notifier).setMarkerIcon(LatLng(
            widget.orderData.location?.latitude ?? AppConstants.demoLatitude,
            widget.orderData.location?.longitude ?? AppConstants.demoLongitude,
          ));
    });
    _target = LatLng(
      widget.orderData.location?.latitude ?? AppConstants.demoLatitude,
      widget.orderData.location?.longitude ?? AppConstants.demoLongitude,
    );
    super.initState();
  }

  @override
  void dispose() {
    mapController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        Padding(
          padding: EdgeInsets.only(left: 28.r, right: 12.r),
          child: Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                AppHelpers.getTranslation(TrKeys.showLocationOnMap),
                style: GoogleFonts.inter(
                  fontSize: 21.sp,
                  color: AppColors.black,
                  fontWeight: FontWeight.w600,
                ),
              ),
              IconButton(
                  splashRadius: 32.r,
                  onPressed: () => Navigator.pop(context),
                  icon: const Icon(FlutterRemix.close_line))
            ],
          ),
        ),
        Padding(
          padding: EdgeInsets.only(left: 28.r, right: 180.r),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              28.verticalSpace,
              OrderStatusScreen(
                status: AppHelpers.getOrderStatus(widget.orderData.status),
              ),
              28.verticalSpace,
              Row(
                children: [
                  Text(
                    AppHelpers.getTranslation(TrKeys.order),
                    style: GoogleFonts.inter(
                      fontSize: 21.sp,
                      color: AppColors.black,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                  5.horizontalSpace,
                  Text(
                    "#ID${widget.orderData.id}",
                    style: GoogleFonts.inter(
                      fontSize: 21.sp,
                      color: AppColors.black,
                      fontWeight: FontWeight.w400,
                    ),
                  ),
                  const Spacer(),
                  InvoiceDownload(orderData: widget.orderData),
                ],
              ),
              7.verticalSpace,
              Row(
                children: [
                  SizedBox(
                    width: 280.r,
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          DateFormat("yyyy MMMM dd, hh:mm:ss").format(
                              DateTime.parse(widget.orderData.createdAt ?? "")),
                          style: GoogleFonts.inter(
                            fontSize: 14.sp,
                            color: AppColors.brandTitleDivider,
                            fontWeight: FontWeight.w400,
                          ),
                        ),
                        8.verticalSpace,
                        Text(
                          "${AppHelpers.getTranslation(TrKeys.scheduletAt)} ${DateFormat("yyyy MMMM dd").format(DateTime.parse(widget.orderData.createdAt ?? ""))}",
                          style: GoogleFonts.inter(
                            fontSize: 14.sp,
                            color: AppColors.brandTitleDivider,
                            fontWeight: FontWeight.w400,
                          ),
                        ),
                        8.verticalSpace,
                        Text(
                          widget.orderData.orderAddress?.address ?? "",
                          style: GoogleFonts.inter(
                            fontSize: 14.sp,
                            color: AppColors.black,
                            fontWeight: FontWeight.w500,
                          ),
                          maxLines: 2,
                        ),
                      ],
                    ),
                  ),
                  Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        children: [
                          Text(
                            AppHelpers.getTranslation(TrKeys.status),
                            style: GoogleFonts.inter(
                              fontSize: 14.sp,
                              color: AppColors.brandTitleDivider,
                              fontWeight: FontWeight.w500,
                            ),
                          ),
                          8.horizontalSpace,
                          AppHelpers.getStatusType(
                              widget.orderData.status ?? ""),
                        ],
                      ),
                      8.verticalSpace,
                      RichText(
                        text: TextSpan(
                            text:
                                "${AppHelpers.getTranslation(TrKeys.paymentType)}  ",
                            style: GoogleFonts.inter(
                              fontSize: 14.sp,
                              color: AppColors.brandTitleDivider,
                              fontWeight: FontWeight.w500,
                            ),
                            children: [
                              TextSpan(
                                text: widget.orderData.transaction
                                        ?.paymentSystem?.tag ??
                                    "- -",
                                style: GoogleFonts.inter(
                                  fontSize: 14.sp,
                                  color: AppColors.black,
                                  fontWeight: FontWeight.w500,
                                ),
                              )
                            ]),
                      ),
                      8.verticalSpace,
                      RichText(
                        text: TextSpan(
                            text:
                                "${AppHelpers.getTranslation(TrKeys.orderType)}  ",
                            style: GoogleFonts.inter(
                              fontSize: 14.sp,
                              color: AppColors.brandTitleDivider,
                              fontWeight: FontWeight.w500,
                            ),
                            children: [
                              TextSpan(
                                text: AppHelpers.getTranslation(
                                    widget.orderData.deliveryType ?? "--"),
                                style: GoogleFonts.inter(
                                  fontSize: 14.sp,
                                  color: AppColors.black,
                                  fontWeight: FontWeight.w500,
                                ),
                              )
                            ]),
                      ),
                    ],
                  ),
                ],
              ),
            ],
          ),
        ),
        32.verticalSpace,
        Expanded(
            child: Stack(
          children: [
            GoogleMap(
              markers: ref.watch(orderTableProvider).setOfMarker,
              tiltGesturesEnabled: false,
              myLocationButtonEnabled: false,
              zoomControlsEnabled: false,
              onMapCreated: (GoogleMapController controller) {
                mapController = controller;
              },
              initialCameraPosition: CameraPosition(
                bearing: 0,
                target: _target,
                tilt: 0,
                zoom: 17,
              ),
            ),
            Positioned(
              bottom: 20.r,
              right: 20.r,
              child: MapButtons(
                zoomIn: () {
                  mapController.animateCamera(CameraUpdate.zoomIn());
                },
                zoomOut: () {
                  mapController.animateCamera(CameraUpdate.zoomOut());
                },
                navigate: () {
                  mapController.animateCamera(
                    CameraUpdate.newCameraPosition(
                      CameraPosition(target: _target, zoom: 17),
                    ),
                  );
                },
              ),
            )
          ],
        ))
      ],
    );
  }
}
