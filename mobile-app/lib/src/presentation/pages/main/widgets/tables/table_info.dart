import 'package:admin_desktop/src/presentation/components/buttons/floor_button.dart';
import 'package:flutter/material.dart';
import 'package:flutter_remix/flutter_remix.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../../../../core/constants/constants.dart';
import '../../../../../core/utils/app_helpers.dart';
import '../../../../components/components.dart';
import '../../../../theme/theme.dart';
import 'riverpod/tables_provider.dart';

class TableInfo extends ConsumerWidget {
  const TableInfo({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context, ref) {
    final notifier = ref.read(tablesProvider.notifier);
    final state = ref.watch(tablesProvider);
    return Container(
      padding: REdgeInsets.all(16),
      decoration: BoxDecoration(
          color: AppColors.white, borderRadius: BorderRadius.circular(10)),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SizedBox(
            height: 48.r,
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                ListView.builder(
                    itemCount: 2,
                    shrinkWrap: true,
                    scrollDirection: Axis.horizontal,
                    itemBuilder: (context, index) {
                      return Row(
                        children: [
                          FloorButton(
                              height: 48,
                              paddingSize: 21,
                              bgColor: AppColors.borderColor,
                              isTab: true,
                              isActive: state.selectFloor == index,
                              title: AppHelpers.getTranslation(
                                  state.listFloor[index]),
                              onTap: () => notifier.changeFloor(index)),
                          16.r.horizontalSpace,
                        ],
                      );
                    }),
                ConfirmButton(
                    icon: Icon(FlutterRemix.add_line, size: 24.r),
                    paddingSize: 16,
                    title: "",
                    onTap: () {}),
              ],
            ),
          ),
          const Spacer(),
          Center(
            child: Text(
              "Coming soon !!!",
              style: GoogleFonts.inter(fontSize: 20.sp),
            ),
          ),
          const Spacer(),
          Padding(
            padding: EdgeInsets.symmetric(vertical: 7.r, horizontal: 7.r),
            child: LoginButton(
                title: AppHelpers.getTranslation(TrKeys.checkIn),
                onPressed: () {}),
          )
        ],
      ),
    );
  }
}
