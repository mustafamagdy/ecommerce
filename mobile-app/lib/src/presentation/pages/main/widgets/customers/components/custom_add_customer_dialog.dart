import 'package:flutter/material.dart';
import 'package:flutter_remix/flutter_remix.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../../../../../core/constants/constants.dart';
import '../../../../../../core/utils/app_helpers.dart';
import '../../../../../components/components.dart';
import '../../../../../theme/app_colors.dart';
import '../riverpod/provider/customer_provider.dart';
import 'custom_button.dart';

class AddCustomerDialog extends ConsumerStatefulWidget {
  const AddCustomerDialog({super.key});

  @override
  ConsumerState<ConsumerStatefulWidget> createState() =>
      _AddCustomDialogState();
}

class _AddCustomDialogState extends ConsumerState<AddCustomerDialog> {
  final GlobalKey<FormState> formKey = GlobalKey<FormState>();
  late TextEditingController firstName;
  late TextEditingController lastName;
  late TextEditingController email;
  late TextEditingController phone;
  late TextEditingController newPassword;
  late TextEditingController confirmPassword;

  @override
  void initState() {
    firstName = TextEditingController();
    lastName = TextEditingController();
    email = TextEditingController();
    newPassword = TextEditingController();
    confirmPassword = TextEditingController();
    phone = TextEditingController();

    super.initState();
  }

  @override
  void dispose() {
    firstName.dispose();
    lastName.dispose();
    email.dispose();
    newPassword.dispose();
    confirmPassword.dispose();
    phone.dispose();

    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final notifier = ref.read(customerProvider.notifier);
    final state = ref.watch(customerProvider);
    return AlertDialog(
      title: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(
            AppHelpers.getTranslation(TrKeys.addCustomer),
            style: GoogleFonts.inter(
                fontSize: 22.sp,
                color: AppColors.black,
                fontWeight: FontWeight.w600),
          ),
          IconButton(
              onPressed: () {
                Navigator.pop(context);
              },
              icon: const Icon(FlutterRemix.close_fill))
        ],
      ),
      content: SingleChildScrollView(
        child: Form(
          key: formKey,
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              OutlinedBorderTextField(
                validator: (s) {
                  if (s?.isEmpty ?? true) {
                    return AppHelpers.getTranslation(TrKeys.enterName);
                  }
                  return null;
                },
                textController: firstName,
                border: AppColors.transparent,
                color: AppColors.editProfileCircle,
                label: '${AppHelpers.getTranslation(TrKeys.firstname)}*',
                style: GoogleFonts.inter(
                    fontSize: 14.sp,
                    color: AppColors.black,
                    fontWeight: FontWeight.w500),
              ),
              12.verticalSpace,
              OutlinedBorderTextField(
                  validator: (s) {
                    if (s?.isEmpty ?? true) {
                      return AppHelpers.getTranslation(TrKeys.enterLastName);
                    }
                    return null;
                  },
                  textController: lastName,
                  border: AppColors.transparent,
                  style: GoogleFonts.inter(
                      fontSize: 14.sp,
                      color: AppColors.black,
                      fontWeight: FontWeight.w500),
                  color: AppColors.editProfileCircle,
                  label: '${AppHelpers.getTranslation(TrKeys.lastname)}*'),
              12.verticalSpace,
              OutlinedBorderTextField(
                  validator: (s) {
                    if (s?.isEmpty ?? true) {
                      return AppHelpers.getTranslation(TrKeys.enterPhone);
                    }
                    return null;
                  },
                  textController: phone,
                  border: AppColors.transparent,
                  style: GoogleFonts.inter(
                      fontSize: 14.sp,
                      color: AppColors.black,
                      fontWeight: FontWeight.w500),
                  color: AppColors.editProfileCircle,
                  label: '${AppHelpers.getTranslation(TrKeys.phone)}*'),
              12.verticalSpace,
              OutlinedBorderTextField(
                  validator: (s) {
                    if (s?.isEmpty ?? true) {
                      return AppHelpers.getTranslation(TrKeys.enterEmail);
                    }
                    return null;
                  },
                  textController: email,
                  border: AppColors.transparent,
                  style: GoogleFonts.inter(
                      fontSize: 14.sp,
                      color: AppColors.black,
                      fontWeight: FontWeight.w500),
                  color: AppColors.editProfileCircle,
                  label: '${AppHelpers.getTranslation(TrKeys.email)}*'),
              12.verticalSpace,
              41.verticalSpace,
              Row(
                children: [
                  SizedBox(
                    height: 36.h,
                    width: 122.w,
                    child: LoginButton(
                        isLoading: state.createUserLoading,
                        title: AppHelpers.getTranslation(TrKeys.save),
                        onPressed: () {
                          if (formKey.currentState?.validate() ?? false) {
                            notifier.createCustomer(context,
                                email: email.text,
                                lastName: lastName.text,
                                name: firstName.text,
                                phone: phone.text, onSuccess: () {
                              Navigator.pop(context);
                            });
                          }
                        }),
                  ),
                  21.horizontalSpace,
                  CustomButton(
                    onTap: () => Navigator.pop(context),
                    containerColor: AppColors.transparent,
                    title: AppHelpers.getTranslation(TrKeys.cancel),
                    textColor: AppColors.black,
                    borderColor: AppColors.iconColor,
                  ),
                ],
              )
            ],
          ),
        ),
      ),
    );
  }
}
