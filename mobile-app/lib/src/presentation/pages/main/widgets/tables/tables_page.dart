
import 'package:admin_desktop/src/models/data/table_model.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/tables/riverpod/tables_notifier.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/tables/riverpod/tables_provider.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/tables/riverpod/tables_state.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/tables/widgets/add_new_table.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/tables/widgets/custom_table.dart';
import 'package:flutter/material.dart';
import 'package:flutter_remix/flutter_remix.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../../../../core/constants/constants.dart';
import '../../../../../core/utils/utils.dart';
import '../../../../components/components.dart';
import '../../../../theme/app_colors.dart';
import 'table_info.dart';
import 'widgets/new_order_screen.dart';

class TablesPage extends StatefulWidget {
  const TablesPage({Key? key}) : super(key: key);

  @override
  State<TablesPage> createState() => _TablesPageState();
}

class _TablesPageState extends State<TablesPage> {
  final GlobalKey _myWidgetKey = GlobalKey();
  Size? _widgetSize;

  @override
  void initState() {
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _widgetSize = _myWidgetKey.currentContext?.size;
      setState(() {});
    });
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: REdgeInsets.all(16),
      child: Row(
        children: [
          Expanded(flex: 2, child: _tablesList()),
          const Expanded(flex: 1, child: TableInfo()),
        ],
      ),
    );
  }

  Widget _tablesList() {
    return Stack(
      children: [
        Padding(
          padding: REdgeInsets.only(left: 16, right: 17),
          child: Column(
            mainAxisSize: MainAxisSize.max,
            crossAxisAlignment: CrossAxisAlignment.start,
            mainAxisAlignment: MainAxisAlignment.start,
            children: [
              Text(
                AppHelpers.getTranslation(TrKeys.tables),
                style: GoogleFonts.inter(
                  fontSize: 22.sp,
                  fontWeight: FontWeight.w600,
                ),
              ),
              16.verticalSpace,
              Consumer(builder: (context, ref, child) {
                return _topWidgets(ref.watch(tablesProvider),
                    ref.read(tablesProvider.notifier));
              }),
              16.verticalSpace,
              Consumer(builder: (context, ref, child) {
                final tableList = ref.watch(tablesProvider).tableList;

                return Expanded(
                  child: SingleChildScrollView(
                    child: Wrap(
                      children: [
                        for (int i = 0; i < tableList.length; i++)
                          Padding(
                            padding: REdgeInsets.only(
                                right: 12, bottom: 12, top: 16),
                            child: Draggable<int>(
                              data: i,
                              feedback: CustomTable(
                                key: _myWidgetKey,
                                tableModel: TableModel(
                                    title: tableList[i]?.title ?? "",
                                    count: tableList[i]?.count ?? 0,
                                    tax: tableList[i]?.tax ?? 0,
                                    zona: tableList[i]?.zona ?? "",
                                    isActive: tableList[i]?.isActive ?? false),
                                tableHeight: _widgetSize?.height == null
                                    ? null
                                    : (MediaQuery.of(context).size.height / 2),
                                tableWidth: _widgetSize?.width == null
                                    ? null
                                    : (_widgetSize!.width / 2),
                              ),
                              childWhenDragging: const SizedBox.shrink(),
                              child: GestureDetector(
                                onTap: () {
                                  AppHelpers.showAlertDialog(
                                      context: context,
                                      child: NewOrderScreen(
                                        tableModel: tableList[i],
                                        index: i,
                                      ));
                                },
                                child: CustomTable(
                                  tableModel: TableModel(
                                      title: tableList[i]?.title ?? "",
                                      count: tableList[i]?.count ?? 0,
                                      tax: tableList[i]?.tax ?? 0,
                                      zona: tableList[i]?.zona ?? "",
                                      isActive:
                                          tableList[i]?.isActive ?? false),
                                ),
                              ),
                            ),
                          ),
                      ],
                    ),
                  ),
                );
              }),
              Container(
                width: double.infinity,
                padding: REdgeInsets.symmetric(vertical: 7, horizontal: 18),
                decoration: BoxDecoration(
                    color: AppColors.white,
                    borderRadius: BorderRadius.circular(10.r)),
                child: Row(
                  children: [
                    Padding(
                      padding: REdgeInsets.symmetric(vertical: 10),
                      child: Text(AppHelpers.getTranslation(TrKeys.tables),
                          style: GoogleFonts.inter(
                            fontSize: 14.sp,
                            fontWeight: FontWeight.w600,
                          )),
                    ),
                    14.horizontalSpace,
                    SizedBox(
                      height: 42.r,
                      child: const VerticalDivider(
                          color: AppColors.hintColor, thickness: 1),
                    ),
                    _tableStatus(
                        tableStatus: TrKeys.available,
                        tableCount: 7,
                        statusColor: AppColors.hintColor),
                    _tableStatus(
                        tableStatus: TrKeys.booked,
                        tableCount: 2,
                        statusColor: AppColors.starColor),
                    _tableStatus(
                        tableStatus: TrKeys.occupied,
                        tableCount: 1,
                        statusColor: AppColors.red),
                  ],
                ),
              )
            ],
          ),
        ),
        Positioned(
          bottom: 0,
          right: 0,
          child: Consumer(builder: (context, ref, child) {
            return DragTarget<int>(
              builder: (
                BuildContext context,
                List<dynamic> accepted,
                List<dynamic> rejected,
              ) {
                return Padding(
                  padding: REdgeInsets.only(
                      top: 100, left: 48, right: 36, bottom: 6),
                  child: Container(
                    height: 42.r,
                    padding: REdgeInsets.symmetric(horizontal: 16),
                    decoration: BoxDecoration(
                      borderRadius: BorderRadius.circular(8),
                      color: AppColors.shimmerBase,
                    ),
                    child: Center(
                      child: Icon(
                        Icons.delete,
                        size: 21.sp,
                        color: AppColors.black,
                      ),
                    ),
                  ),
                );
              },
              onAccept: (int index) {
                debugPrint(index.toString());
                ref.read(tablesProvider.notifier).deleteTable(index);
              },
            );
          }),
        ),
      ],
    );
  }

  _tableStatus({
    required String tableStatus,
    required int tableCount,
    required Color statusColor,
  }) {
    return Padding(
      padding: REdgeInsets.symmetric(horizontal: 14),
      child: Row(
        children: [
          Container(
            height: 14.r,
            width: 14.r,
            decoration: BoxDecoration(
              shape: BoxShape.circle,
              color: statusColor,
            ),
          ),
          6.r.horizontalSpace,
          Text(
            "${AppHelpers.getTranslation(tableStatus)} : $tableCount",
            style: GoogleFonts.inter(
                fontSize: 12.sp,
                fontWeight: FontWeight.w500,
                color: AppColors.reviewText),
          ),
        ],
      ),
    );
  }

  Widget _topWidgets(TablesState state, TablesNotifier notifier) {
    List statusList = [
      TrKeys.allTables,
      TrKeys.available,
      TrKeys.booked,
      TrKeys.occupied,
    ];
    return Row(
      children: [
        for (int i = 0; i < statusList.length; i++)
          Padding(
            padding: REdgeInsets.only(left: 8),
            child: ConfirmButton(
              paddingSize: 18,
              textSize: 14,
              isActive: state.selectTabIndex == i,
              title: AppHelpers.getTranslation(statusList[i]),
              textColor: AppColors.black,
              isTab: true,
              isShadow: true,
              onTap: () => notifier.changeIndex(i),
            ),
          ),
        const Spacer(),
        ConfirmButton(
          paddingSize: 20,
          prefixIcon: Icon(FlutterRemix.add_fill, size: 28.r),
          textSize: 14,
          title: AppHelpers.getTranslation(TrKeys.addNewTable),
          textColor: AppColors.black,
          onTap: () {
            AppHelpers.showAlertDialog(
                context: context, child: const AddNewTable());
          },
        )
      ],
    );
  }
}
