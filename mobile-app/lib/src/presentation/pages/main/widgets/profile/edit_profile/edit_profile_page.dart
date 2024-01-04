import 'package:admin_desktop/src/models/models.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/profile/edit_profile/riverpod/provider/edit_profile_provider.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/profile/edit_profile/widgets/custom_date_textform.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/profile/edit_profile/widgets/label_and_textform.dart';
import 'package:flutter/material.dart';
import 'package:flutter_remix/flutter_remix.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../../../../../core/constants/constants.dart';
import '../../../../../../core/utils/app_helpers.dart';
import '../../../../../../core/utils/local_storage.dart';
import '../../../../../components/buttons/circle_choosing_button.dart';
import '../../../../../components/components.dart';
import '../../../../../components/custom_edit_profile_widget.dart';
import '../../../../../theme/app_colors.dart';
import 'package:intl/intl.dart' as intl;

class ProfilePage extends ConsumerStatefulWidget {
  const ProfilePage({super.key});

  @override
  ConsumerState<ConsumerStatefulWidget> createState() => _ProfilePageState();
}

class _ProfilePageState extends ConsumerState<ProfilePage> {
  late TextEditingController firstName;
  late TextEditingController lastName;
  late TextEditingController email;
  late TextEditingController confirmPassword;
  late TextEditingController phoneNumber;
  late TextEditingController newPassw;
  late TextEditingController dateOfBirth;
  late TextEditingController personalPincode;

  @override
  void initState() {
    firstName = TextEditingController();
    lastName = TextEditingController();
    email = TextEditingController();
    phoneNumber = TextEditingController();
    dateOfBirth = TextEditingController();
    confirmPassword = TextEditingController();
    newPassw = TextEditingController();
    personalPincode = TextEditingController();
    getInfos();

    super.initState();
  }

  @override
  void dispose() {
    firstName.dispose();
    lastName.dispose();
    email.dispose();
    phoneNumber.dispose();
    dateOfBirth.dispose();
    confirmPassword.dispose();
    newPassw.dispose();
    personalPincode.dispose();
    super.dispose();
  }

  getInfos() {
    firstName.text = LocalStorage.instance.getUser()?.firstname ?? '';
    lastName.text = LocalStorage.instance.getUser()?.lastname ?? '';
    dateOfBirth.text = LocalStorage.instance.getUser()?.birthday ?? '';
    firstName.text = LocalStorage.instance.getUser()?.firstname ?? '';
    phoneNumber.text = LocalStorage.instance.getUser()?.phone ?? '';
    email.text = LocalStorage.instance.getUser()?.email ?? '';
    dateOfBirth.text = intl.DateFormat("yyy-MM-dd").format(
        DateTime.tryParse(LocalStorage.instance.getUser()?.birthday ?? '') ??
            DateTime.now());
    ref
        .read(editProfileProvider.notifier)
        .setGender(LocalStorage.instance.getUser()?.gender ?? '');
  }

  @override
  Widget build(BuildContext context) {
    final notifier = ref.read(editProfileProvider.notifier);
    final state = ref.watch(editProfileProvider);
    return Padding(
      padding: const EdgeInsets.only(left: 16, top: 24, bottom: 16, right: 15),
      child: Container(
        decoration: const BoxDecoration(
            color: AppColors.white,
            borderRadius: BorderRadius.all(Radius.circular(10))),
        child: Padding(
          padding:  EdgeInsets.only(left: 16.r),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              24.verticalSpace,
              Row(
                children: [
                  CustomEditWidget(
                      image: state.url,
                      imagePath: state.imagePath,
                      isEmptyorNot:
                          (LocalStorage.instance.getUser()?.img?.isNotEmpty ??
                                  false) &&
                              state.imagePath.isEmpty,
                      isEmptyorNot2: state.imagePath.isNotEmpty,
                      localStoreImage:
                          LocalStorage.instance.getUser()?.img ?? "",
                      onthisTap: () {
                        notifier.getPhoto();
                      }),
                  Padding(
                    padding: const EdgeInsets.only(left: 28),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          '${LocalStorage.instance.getUser()?.firstname} ${LocalStorage.instance.getUser()?.lastname?.substring(0, 1).toUpperCase()}.',
                          style: GoogleFonts.inter(
                              fontSize: 24.sp,
                              color: AppColors.black,
                              fontWeight: FontWeight.bold),
                        ),
                        8.verticalSpace,
                        Text(
                          '${LocalStorage.instance.getUser()?.role}',
                          style: GoogleFonts.inter(
                              fontSize: 18.sp,
                              color: AppColors.iconColor,
                              fontWeight: FontWeight.bold),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
              42.verticalSpace,
              SizedBox(
                height: 18,
                child: ListView.builder(
                  physics: const NeverScrollableScrollPhysics(),
                  scrollDirection: Axis.horizontal,
                  itemCount: 2,
                  itemBuilder: (BuildContext context, int index) => Row(
                    children: [
                      InkWell(
                        onTap: () {
                          ref
                              .read(editProfileProvider.notifier)
                              .changeIndex(index);
                          notifier.setGender(index == 0 ? 'male' : 'female');
                        },
                        child: CircleChoosingButton(
                          isActive: state.selectIndex == index,
                        ),
                      ),
                      12.horizontalSpace,
                      Padding(
                        padding: const EdgeInsets.only(right: 32),
                        child: Text(
                          index == 0
                              ? AppHelpers.getTranslation(TrKeys.male)
                              : AppHelpers.getTranslation(TrKeys.female),
                          style: GoogleFonts.inter(
                              fontSize: 15.sp,
                              color: AppColors.black,
                              fontWeight: FontWeight.w500),
                        ),
                      )
                    ],
                  ),
                ),
              ),
              18.verticalSpace,
              Row(
                children: [
                  Expanded(
                    child: Padding(
                      padding: const EdgeInsets.only(right: 80),
                      child: CustomColumnWidget(
                        controller: firstName,
                        trName: AppHelpers.getTranslation(TrKeys.firstname),
                      ),
                    ),
                  ),
                  Expanded(
                    child: Padding(
                      padding: const EdgeInsets.only(right: 16),
                      child: CustomColumnWidget(
                        controller: lastName,
                        trName: AppHelpers.getTranslation(TrKeys.lastname),
                      ),
                    ),
                  )
                ],
              ),
              24.verticalSpace,
              Row(
                children: [
                  Expanded(
                    child: Padding(
                      padding: const EdgeInsets.only(right: 80),
                      child: CustomColumnWidget(
                        inputType: TextInputType.emailAddress,
                        controller: email,
                        trName: AppHelpers.getTranslation(TrKeys.email),
                      ),
                    ),
                  ),
                  Expanded(
                    child: Padding(
                      padding: const EdgeInsets.only(right: 16),
                      child: CustomColumnWidget(
                        trName: AppHelpers.getTranslation(TrKeys.newPassword),
                        controller: confirmPassword,
                        obscure: state.showOldPassword,
                        suffixIcon: GestureDetector(
                          child: Icon(
                            state.showOldPassword
                                ? FlutterRemix.eye_line
                                : FlutterRemix.eye_close_line,
                            color: AppColors.black,
                            size: 20.r,
                          ),
                          onTap: () => notifier
                              .setShowOldPassword(!state.showOldPassword),
                        ),
                      ),
                    ),
                  )
                ],
              ),
              24.verticalSpace,
              Row(
                children: [
                  Expanded(
                    child: Padding(
                      padding: const EdgeInsets.only(right: 80),
                      child: CustomColumnWidget(
                        inputType: TextInputType.phone,
                        controller: phoneNumber,
                        trName: AppHelpers.getTranslation(TrKeys.phone),
                      ),
                    ),
                  ),
                  Expanded(
                    child: Padding(
                      padding: const EdgeInsets.only(right: 16),
                      child: CustomColumnWidget(
                        trName: AppHelpers.getTranslation(
                            TrKeys.confirmNewPassword),
                        controller: newPassw,
                        obscure: state.showPassword,
                        suffixIcon: IconButton(
                          splashRadius: 25.r,
                          icon: Icon(
                            state.showPassword
                                ? FlutterRemix.eye_line
                                : FlutterRemix.eye_close_line,
                            color: AppColors.black,
                            size: 20.r,
                          ),
                          onPressed: () =>
                              notifier.setShowPassword(!state.showPassword),
                        ),
                      ),
                    ),
                  ),
                ],
              ),
              24.verticalSpace,
              Row(
                children: [
                  Expanded(
                    child: Padding(
                      padding: const EdgeInsets.only(right: 80),
                      child: CustomDateFormField(
                        controller: dateOfBirth,
                        text: AppHelpers.getTranslation(TrKeys.dateOfBirth),
                      ),
                    ),
                  ),
                  Expanded(
                    child: Padding(
                      padding: const EdgeInsets.only(right: 16),
                      child: CustomColumnWidget(
                        maxLength: 4,
                        trName:
                            AppHelpers.getTranslation(TrKeys.personalPincode),
                        controller: personalPincode,
                        inputType: TextInputType.number,
                        obscure: state.showPincode,
                        suffixIcon: IconButton(
                          splashRadius: 25.r,
                          icon: Icon(
                            state.showPincode
                                ? FlutterRemix.eye_line
                                : FlutterRemix.eye_close_line,
                            color: AppColors.black,
                            size: 20.r,
                          ),
                          onPressed: () => notifier
                              .setShowPersonalPincode(!state.showPincode),
                        ),
                      ),
                    ),
                  )
                ],
              ),
           const Spacer(),
              SizedBox(
                width: 250.w,
                child: LoginButton(
                    isLoading: state.isLoading,
                    title: AppHelpers.getTranslation(TrKeys.save),
                    onPressed: () {
                      notifier.editProfile(
                          context,
                          UserData(
                            birthday: dateOfBirth.text,
                            img: state.imagePath,
                            firstname: firstName.text,
                            lastname: lastName.text,
                            email: email.text,
                            phone: phoneNumber.text,
                          ));
                      if (newPassw.text.isNotEmpty &&
                          confirmPassword.text.isNotEmpty) {
                        notifier.updatePassword(context,
                            password: confirmPassword.text,
                            confirmPassword: newPassw.text);
                      }
                      if (personalPincode.text.isNotEmpty &&
                          personalPincode.text.length == 4) {
                        LocalStorage.instance.setPinCode(personalPincode.text);
                      }
                    }),
              ),
              16.verticalSpace,
            ],
          ),
        ),
      ),
    );
  }
}
