import 'package:admin_desktop/generated/assets.dart';
import 'package:admin_desktop/src/core/utils/app_helpers.dart';
import 'package:admin_desktop/src/core/utils/validator_utils.dart';
import 'package:admin_desktop/src/models/models.dart';
import 'package:admin_desktop/src/presentation/components/components.dart';
import 'package:auto_route/auto_route.dart';
import 'package:flutter/material.dart';
import 'package:flutter_remix/flutter_remix.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:intl/intl.dart';

import '../../../../../../core/constants/constants.dart';
import '../../../../../theme/app_colors.dart';
import '../riverpod/tables_provider.dart';
import 'table_form_field.dart';

class NewOrderScreen extends ConsumerStatefulWidget {
  final TableModel? tableModel;
  final int index;

  const NewOrderScreen({
    Key? key,
    this.tableModel,
    required this.index,
  }) : super(key: key);

  @override
  ConsumerState<NewOrderScreen> createState() => _NewOrderScreenState();
}

class _NewOrderScreenState extends ConsumerState<NewOrderScreen> {
  final GlobalKey<FormState> formKey = GlobalKey<FormState>();
  late TextEditingController name;
  late TextEditingController count;
  late TextEditingController time;
  late TextEditingController date;

  @override
  void initState() {
    name = TextEditingController(text: widget.tableModel?.title);
    count = TextEditingController(text: widget.tableModel?.count.toString());
    time = TextEditingController();
    date = TextEditingController();
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    final notifier = ref.read(tablesProvider.notifier);
    return SizedBox(
      height: 400.r,
      child: Column(
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Padding(
                padding: REdgeInsets.only(top: 5),
                child: Text(
                  AppHelpers.getTranslation(TrKeys.newOrder),
                  style: GoogleFonts.inter(
                    color: AppColors.black,
                    fontWeight: FontWeight.w500,
                    fontSize: 16.sp,
                  ),
                ),
              ),
              GestureDetector(
                  onTap: () => Navigator.pop(context),
                  child: Icon(Icons.close, color: AppColors.black, size: 24.r)),
            ],
          ),
          Expanded(
            child: Form(
              key: formKey,
              child: SingleChildScrollView(
                child: Column(
                  children: [
                    24.r.verticalSpace,
                    TableFormField(
                      prefixSvg: Assets.svgDine,
                      validator: ValidatorUtils.validateEmpty,
                      hintText: TrKeys.tableName,
                      textEditingController: name,
                      readOnly: true,
                    ),
                    12.r.verticalSpace,
                    TableFormField(
                      prefixSvg: Assets.svgAvatar,
                      inputType: TextInputType.number,
                      validator: ValidatorUtils.validateEmpty,
                      hintText: TrKeys.personCount,
                      textEditingController: count,
                      readOnly: true,
                    ),
                    12.r.verticalSpace,
                    TableFormField(
                      onTap: () {
                        showDatePicker(
                          context: context,
                          initialDate: DateTime.now(),
                          firstDate: DateTime.now(),
                          lastDate: DateTime.now().add(
                            const Duration(days: 1000),
                          ),
                          builder: (context, child) {
                            return Theme(
                              data: Theme.of(context).copyWith(
                                colorScheme: const ColorScheme.light(
                                  primary: AppColors.brandColor,
                                  onPrimary: AppColors.black,
                                  onSurface: AppColors.black,
                                ),
                                textButtonTheme: TextButtonThemeData(
                                  style: TextButton.styleFrom(
                                    foregroundColor: AppColors.black,
                                  ),
                                ),
                              ),
                              child: child!,
                            );
                          },
                        ).then((selectDate) {
                          if (selectDate != null) {
                            date.text =
                                DateFormat("MM/dd/yyyy").format(selectDate);
                          }
                        });
                      },
                      prefixIcon: FlutterRemix.calendar_check_line,
                      prefixSvg: Assets.svgTax,
                      inputType: TextInputType.number,
                      validator: ValidatorUtils.validateEmpty,
                      hintText: DateFormat("MM/dd/yyyy").format(DateTime.now()),
                      textEditingController: date,
                      readOnly: true,
                    ),
                    12.r.verticalSpace,
                    TableFormField(
                      onTap: () {
                        showTimePicker(
                          context: context,
                          initialTime: TimeOfDay.now(),
                          builder: (context, child) {
                            return Theme(
                              data: Theme.of(context).copyWith(
                                colorScheme: const ColorScheme.light(
                                  primary: AppColors.brandColor,
                                  onPrimary: AppColors.black,
                                  onSurface: AppColors.black,
                                ),
                                textButtonTheme: TextButtonThemeData(
                                  style: TextButton.styleFrom(
                                    foregroundColor: AppColors.black,
                                  ),
                                ),
                              ),
                              child: child!,
                            );
                          },
                        ).then((selectTime) {
                          if (selectTime != null) {
                            time.text = selectTime.format(context);
                          }
                        });
                      },
                      prefixIcon: FlutterRemix.time_line,
                      prefixSvg: Assets.svgTax,
                      inputType: TextInputType.number,
                      validator: ValidatorUtils.validateEmpty,
                      hintText: TimeOfDay.now().format(context),
                      textEditingController: time,
                      readOnly: true,
                    ),
                    30.verticalSpace,
                    LoginButton(
                        title: AppHelpers.getTranslation(TrKeys.confirm),
                        onPressed: () {
                          if (formKey.currentState?.validate() ?? false) {
                            notifier.changeActive(widget.index,widget.tableModel);
                            context.popRoute();
                          }
                        }),
                  ],
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}
