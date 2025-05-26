part of 'list_view.dart';

class ListMainItem extends ConsumerWidget {
  final List<OrderData> orderList;
  final Color color;
  final bool hasMore;
  final bool isLoading;
  final VoidCallback onViewMore;

  const ListMainItem({
    Key? key,
    required this.orderList,
    required this.color,
    required this.hasMore,
    required this.onViewMore,
    required this.isLoading,
  }) : super(key: key);

  @override
  Widget build(BuildContext context, ref) {
    final notifier = ref.read(orderTableProvider.notifier);
    final state = ref.watch(orderTableProvider);
    return Column(
      children: [
        Container(
          color: AppColors.white,
          padding: EdgeInsets.symmetric(vertical: 18.r, horizontal: 18.r),
          child: Row(
            children: [
              CustomCheckbox(
                isActive: state.isAllSelect,
                onTap: () => notifier.allSelectOrder(orderList),
              ),
              8.horizontalSpace,
              SizedBox(
                width: 56.w,
                child: Text(
                  AppHelpers.getTranslation(TrKeys.id),
                  style: GoogleFonts.inter(
                    fontSize: 16.sp,
                    color: AppColors.black,
                    fontWeight: FontWeight.w500,
                  ),
                ),
              ),
              8.horizontalSpace,
              SizedBox(
                width: 120.w,
                child: Text(
                  AppHelpers.getTranslation(TrKeys.client),
                  style: GoogleFonts.inter(
                    fontSize: 16.sp,
                    color: AppColors.black,
                    fontWeight: FontWeight.w500,
                  ),
                ),
              ),
              8.horizontalSpace,
              SizedBox(
                width: 120.w,
                child: Text(
                  AppHelpers.getTranslation(TrKeys.status),
                  style: GoogleFonts.inter(
                    fontSize: 16.sp,
                    color: AppColors.black,
                    fontWeight: FontWeight.w500,
                  ),
                ),
              ),
              8.horizontalSpace,
              SizedBox(
                width: 120.w,
                child: Text(
                  AppHelpers.getTranslation(TrKeys.deliveryman),
                  style: GoogleFonts.inter(
                    fontSize: 16.sp,
                    color: AppColors.black,
                    fontWeight: FontWeight.w500,
                  ),
                ),
              ),
              8.horizontalSpace,
              SizedBox(
                width: 96.w,
                child: Text(
                  AppHelpers.getTranslation(TrKeys.amount),
                  style: GoogleFonts.inter(
                    fontSize: 16.sp,
                    color: AppColors.black,
                    fontWeight: FontWeight.w500,
                  ),
                ),
              ),
              8.horizontalSpace,
              SizedBox(
                width: 180.w,
                child: Text(
                  AppHelpers.getTranslation(TrKeys.orderTime),
                  style: GoogleFonts.inter(
                    fontSize: 16.sp,
                    color: AppColors.black,
                    fontWeight: FontWeight.w500,
                  ),
                ),
              ),
              8.horizontalSpace,
              SizedBox(
                width: 180.w,
                child: Text(
                  AppHelpers.getTranslation(TrKeys.deliveryDate),
                  style: GoogleFonts.inter(
                    fontSize: 16.sp,
                    color: AppColors.black,
                    fontWeight: FontWeight.w500,
                  ),
                ),
              ),
              8.horizontalSpace,
            ],
          ),
        ),
        const Divider(height: 2),
        Expanded(
          child: orderList.isNotEmpty || isLoading
              ? ListView(
                  children: [
                    ListView.builder(
                        shrinkWrap: true,
                        physics: const NeverScrollableScrollPhysics(),
                        itemCount: orderList.length,
                        itemBuilder: (context, index) {
                          return ListItem(
                            orderData: orderList[index],
                            color: color,
                            onSelect: () => notifier.addSelectOrder(
                                id: orderList[index].id,
                                orderLength: orderList.length),
                            isSelect: state.selectOrders
                                .contains(orderList[index].id),
                          );
                        }),
                    if (isLoading)
                      ListView.builder(
                        physics: const NeverScrollableScrollPhysics(),
                        shrinkWrap: true,
                          itemCount: 6,
                          itemBuilder: (context, index) {
                            return Column(
                              children: [
                                Container(
                                  width: double.infinity,
                                  height: 72.r,
                                  decoration: BoxDecoration(
                                    borderRadius: BorderRadius.circular(1),
                                    color: AppColors.shimmerBase,
                                  ),
                                ),
                                Divider(height: 2.r, color: AppColors.white),
                              ],
                            );
                          }),
                    24.verticalSpace,
                    (hasMore
                        ? InkWell(
                            borderRadius: BorderRadius.circular(10.r),
                            onTap: () => onViewMore(),
                            child: Container(
                              height: 50.r,
                              decoration: BoxDecoration(
                                borderRadius: BorderRadius.circular(10.r),
                                border: Border.all(
                                  color: AppColors.black.withOpacity(0.17),
                                  width: 1.r,
                                ),
                              ),
                              alignment: Alignment.center,
                              child: Text(
                                AppHelpers.getTranslation(TrKeys.viewMore),
                                style: GoogleFonts.inter(
                                  fontSize: 16.sp,
                                  color: AppColors.black,
                                  fontWeight: FontWeight.w500,
                                ),
                              ),
                            ),
                          )
                        : const SizedBox())
                  ],
                )
              : Center(
                  child: Padding(
                    padding: EdgeInsets.only(top: 16.r, bottom: 200.r),
                    child: Text(
                      AppHelpers.getTranslation(TrKeys.emptyOrders),
                    ),
                  ),
                ),
        )
      ],
    );
  }
}
