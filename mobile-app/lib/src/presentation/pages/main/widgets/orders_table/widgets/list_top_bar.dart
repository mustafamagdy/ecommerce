part of 'list_view.dart';

class ListTopBar extends StatelessWidget {
  final String title;
  final String count;
  final VoidCallback onTap;
  final VoidCallback onRefresh;
  final bool isLoading;
  final bool isActive;
  final Color color;

  const ListTopBar({
    Key? key,
    required this.title,
    required this.count,
    required this.onTap,
    required this.isLoading,
    required this.color,
    required this.isActive,
    required this.onRefresh,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return InkWell(
      borderRadius: BorderRadius.vertical(top: Radius.circular(16.r)),
      onTap: onTap,
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 500),
        height: 64.r,
        decoration: BoxDecoration(
          color: isActive ? AppColors.white : AppColors.transparent,
          borderRadius: BorderRadius.only(
              topLeft: Radius.circular(16.r), topRight: Radius.circular(16.r)),
        ),
        child: Padding(
          padding:  EdgeInsets.symmetric(horizontal: 16.r),
          child: Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            crossAxisAlignment: CrossAxisAlignment.center,
            children: [
              Text(
                title,
                style: GoogleFonts.inter(
                  fontSize: 16.sp,
                  fontWeight: FontWeight.w600,
                  color: AppColors.black,
                ),
              ),
              12.horizontalSpace,
              Container(
                padding: EdgeInsets.symmetric(vertical: 6.r, horizontal: 16.r),
                decoration: BoxDecoration(
                    borderRadius: BorderRadius.circular(100.r), color: color),
                child: Text(
                  count,
                  style: GoogleFonts.inter(
                    fontSize: 16.sp,
                    fontWeight: FontWeight.w600,
                    color: AppColors.white,
                  ),
                ),
              ),
              12.horizontalSpace,
              AnimatedContainer(
                duration: const Duration(milliseconds: 500),
                child: isActive
                    ? GestureDetector(
                        onTap: onRefresh,
                        child: isLoading
                            ? Lottie.asset(
                                Assets.lottieRefresh,
                                width: 32.r,
                                height: 32.r,
                                fit: BoxFit.fill,
                              )
                            : const Icon(FlutterRemix.refresh_line),
                      )
                    : const SizedBox.shrink(),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
