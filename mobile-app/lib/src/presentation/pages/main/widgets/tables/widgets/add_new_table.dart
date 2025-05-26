import 'package:admin_desktop/generated/assets.dart';
import 'package:admin_desktop/src/models/data/table_model.dart';
import 'package:admin_desktop/src/presentation/components/components.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/tables/widgets/table_form_field.dart';
import 'package:admin_desktop/src/presentation/theme/app_colors.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../../../../../core/constants/constants.dart';
import '../../../../../../core/utils/utils.dart';
import '../../../../../../core/utils/validator_utils.dart';
import '../riverpod/tables_provider.dart';
import 'custom_drop_down_field.dart';

class AddNewTable extends ConsumerStatefulWidget {
  const AddNewTable({Key? key}) : super(key: key);

  @override
  ConsumerState<AddNewTable> createState() => _AddNewTableState();
}

class _AddNewTableState extends ConsumerState<AddNewTable> {
  final GlobalKey<FormState> formKey = GlobalKey<FormState>();
  late TextEditingController name;
  late TextEditingController count;
  late TextEditingController tax;

  @override
  void initState() {
    name = TextEditingController();
    count = TextEditingController();
    tax = TextEditingController();
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    final notifier = ref.read(tablesProvider.notifier);
    final state = ref.watch(tablesProvider);
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
                  AppHelpers.getTranslation(TrKeys.addNewTable),
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
                    CustomDropDownField(
                      list: state.listFloor,
                      onChanged: (value) {
                        notifier.setFloor(value.toString());
                      },
                      value: state.listFloor.first,
                    ),
                    12.r.verticalSpace,
                     TableFormField(
                      prefixSvg: Assets.svgDine,
                      validator: ValidatorUtils.validateEmpty,
                      hintText: TrKeys.tableName,
                       textEditingController: name,

                    ),
                    12.r.verticalSpace,
                     TableFormField(
                      prefixSvg: Assets.svgAvatar,
                      inputType: TextInputType.number,
                      validator: ValidatorUtils.validateEmpty,
                      hintText: TrKeys.personCount,
                      textEditingController: count,
                    ),
                    12.r.verticalSpace,
                     TableFormField(
                      prefixSvg: Assets.svgTax,
                      inputType: TextInputType.number,
                      validator: ValidatorUtils.validateEmpty,
                      hintText: TrKeys.tax,
                       textEditingController: tax,
                    ),
                    30.verticalSpace,
                    LoginButton(
                        title: AppHelpers.getTranslation(TrKeys.create),
                        onPressed: () {

                          if (formKey.currentState?.validate() ?? false) {
                            notifier.addTable(TableModel(
                                title: name.text,
                                count: int.tryParse(count.text) ?? 4,
                                tax: int.tryParse(tax.text) ?? 0,
                                zona: state.addFloor));
                            Navigator.pop(context);
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
