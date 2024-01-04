import 'package:admin_desktop/src/core/utils/app_helpers.dart';
import 'package:admin_desktop/src/presentation/components/components.dart';
import 'package:admin_desktop/src/presentation/components/shimmers/lines_shimmer.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/customers/components/customers_list.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/customers/components/not_found.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/customers/riverpod/provider/customer_provider.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/notifications/components/view_more_button.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:flutter_staggered_animations/flutter_staggered_animations.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:pull_to_refresh/pull_to_refresh.dart';
import '../../../../../core/constants/constants.dart';
import '../../../../theme/app_colors.dart';
import 'components/custom_add_customer_dialog.dart';
import 'view_customer.dart';

class CustomersPage extends ConsumerStatefulWidget {
  const CustomersPage({super.key});

  @override
  ConsumerState<ConsumerStatefulWidget> createState() => _CustomersPageState();
}

class _CustomersPageState extends ConsumerState<CustomersPage> {
  RefreshController refreshController = RefreshController();

  @override
  void initState() {
    ref.read(customerProvider.notifier).fetchAllUsers();

    super.initState();
  }

  @override
  void dispose() {
    refreshController.dispose();

    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final notifier = ref.read(customerProvider.notifier);
    final state = ref.watch(customerProvider);
    return Padding(
      padding: EdgeInsets.symmetric(horizontal: 16.r),
      child: state.isLoading
          ? Center(
              child: CircularProgressIndicator(
                strokeWidth: 3.r,
                color: AppColors.black,
              ),
            )
          : state.selectUser == null
              ? state.users.isNotEmpty
                  ? ListView(
                      padding: EdgeInsets.only(top: 16.r),
                      shrinkWrap: true,
                      children: [
                        state.query.isNotEmpty
                            ? const SizedBox.shrink()
                            : Row(
                                mainAxisAlignment:
                                    MainAxisAlignment.spaceBetween,
                                children: [
                                  Text(
                                    AppHelpers.getTranslation(TrKeys.customers),
                                    style: GoogleFonts.inter(
                                        fontSize: 22.sp,
                                        color: AppColors.black,
                                        fontWeight: FontWeight.w600),
                                  ),
                                  IconTextButton(
                                      radius: BorderRadius.circular(10.r),
                                      height: 56.h,
                                      backgroundColor: AppColors.brandColor,
                                      iconData: Icons.add,
                                      iconColor: AppColors.black,
                                      title: AppHelpers.getTranslation(
                                          TrKeys.addCustomer),
                                      textColor: AppColors.black,
                                      onPressed: () {
                                        showDialog(
                                          context: context,
                                          builder: (_) =>
                                              const AddCustomerDialog(),
                                        );
                                      })
                                ],
                              ),
                        22.verticalSpace,
                        InkWell(
                          onTap: () {
                            notifier.setUser(state.users[0]);
                          },
                          child: state.query.isNotEmpty
                              ? const SizedBox.shrink()
                              : Container(
                                  height: 164.h,
                                  decoration: BoxDecoration(
                                      color: AppColors.white,
                                      borderRadius:
                                          BorderRadius.circular(10.r)),
                                  child: Row(
                                    children: [
                                      16.horizontalSpace,
                                      CommonImage(
                                        width: 108,
                                        height: 108,
                                        radius: 54,
                                        imageUrl: state.users[0].img ?? '',
                                      ),
                                      20.horizontalSpace,
                                      Padding(
                                        padding: const EdgeInsets.only(top: 41),
                                        child: Padding(
                                          padding:
                                              const EdgeInsets.only(left: 20),
                                          child: Column(
                                            crossAxisAlignment:
                                                CrossAxisAlignment.start,
                                            children: [
                                              Row(
                                                children: [
                                                  Text(
                                                    '${state.users[0].firstname} ${state.users[0].lastname?.substring(0, 1)}.',
                                                    style: GoogleFonts.inter(
                                                        fontSize: 24.sp,
                                                        color: AppColors.black,
                                                        fontWeight:
                                                            FontWeight.w600),
                                                  ),
                                                  12.horizontalSpace,
                                                  Text(
                                                    '#${AppHelpers.getTranslation(TrKeys.id)}${state.users[0].id}',
                                                    style: GoogleFonts.inter(
                                                        fontSize: 16.sp,
                                                        color:
                                                            AppColors.iconColor,
                                                        fontWeight:
                                                            FontWeight.w500),
                                                  ),
                                                ],
                                              ),
                                              12.verticalSpace,
                                              Text(
                                                state.users[0].email ?? '',
                                                style: GoogleFonts.inter(
                                                    fontSize: 14.sp,
                                                    color: AppColors.iconColor,
                                                    fontWeight:
                                                        FontWeight.w500),
                                              ),
                                              6.verticalSpace,
                                              Text(
                                                state.users[0].phone ?? '',
                                                style: GoogleFonts.inter(
                                                    fontSize: 14.sp,
                                                    color: AppColors.iconColor,
                                                    fontWeight:
                                                        FontWeight.w500),
                                              ),
                                            ],
                                          ),
                                        ),
                                      ),
                                    ],
                                  ),
                                ),
                        ),
                        state.query.isNotEmpty
                            ? 1.verticalSpace
                            : 16.verticalSpace,
                        Container(
                          width: double.infinity,
                          decoration: const BoxDecoration(
                              color: AppColors.white,
                              borderRadius: BorderRadius.only(
                                  topLeft: Radius.circular(10),
                                  topRight: Radius.circular(10))),
                          child: Padding(
                            padding: EdgeInsets.symmetric(horizontal: 16.r),
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                20.verticalSpace,
                                Text(
                                  state.query.isNotEmpty
                                      ? AppHelpers.getTranslation(
                                          TrKeys.searchResults)
                                      : AppHelpers.getTranslation(
                                          TrKeys.recentCustomers),
                                  style: GoogleFonts.inter(
                                      fontSize: 22.sp,
                                      color: AppColors.black,
                                      fontWeight: FontWeight.w600),
                                ),
                                16.verticalSpace,
                                Column(
                                  children: [
                                    AnimationLimiter(
                                      child: ListView.builder(
                                        physics:
                                            const NeverScrollableScrollPhysics(),
                                        shrinkWrap: true,
                                        itemCount: state.query.isNotEmpty
                                            ? state.users.length
                                            : state.users.length - 1,
                                        itemBuilder:
                                            (BuildContext context, int index) =>
                                                InkWell(
                                          onTap: () {
                                            notifier.setUser(
                                                state.query.isNotEmpty
                                                    ? state.users[index]
                                                    : state.users[index + 1]);
                                          },
                                          child: AnimationConfiguration
                                              .staggeredList(
                                            position: index,
                                            duration: const Duration(
                                                milliseconds: 375),
                                            child: FadeInAnimation(
                                              child: SlideAnimation(
                                                child: CustomersList(
                                                  imageUrl: state
                                                          .query.isNotEmpty
                                                      ? state.users[index].img
                                                      : state.users[index + 1]
                                                              .img ??
                                                          '',
                                                  name: state.query.isNotEmpty
                                                      ? state.users[index]
                                                          .firstname
                                                      : state.users[index + 1]
                                                              .firstname ??
                                                          '',
                                                  lastname: state
                                                          .query.isNotEmpty
                                                      ? state
                                                          .users[index].lastname
                                                      : state.users[index + 1]
                                                              .lastname ??
                                                          '',
                                                  gmail: state.query.isNotEmpty
                                                      ? state.users[index].email
                                                      : state.users[index + 1]
                                                              .email ??
                                                          '',
                                                  date: state.query.isNotEmpty
                                                      ? state.users[index]
                                                          .registeredAt
                                                      : state.users[index + 1]
                                                          .registeredAt,
                                                ),
                                              ),
                                            ),
                                          ),
                                        ),
                                      ),
                                    ),
                                    28.verticalSpace,
                                    state.isMoreLoading
                                        ? const LineShimmer(
                                            isActiveLine: false,
                                          )
                                        : state.hasMore &&
                                                state.users.isNotEmpty
                                            ? ViewMoreButton(
                                                onTap: () {
                                                  notifier.fetchAllUsers();
                                                },
                                              )
                                            : const SizedBox(),
                                    28.verticalSpace,
                                  ],
                                )
                              ],
                            ),
                          ),
                        )
                      ],
                    )
                  : const Center(child: NotFound())
              : ViewCustomer(
                  user: state.selectUser,
                  back: () {
                    notifier.setUser(null);
                  },
                ),
    );
  }
}
