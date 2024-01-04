import 'package:admin_desktop/src/core/constants/constants.dart';
import 'package:admin_desktop/src/core/utils/app_helpers.dart';
import 'package:admin_desktop/src/presentation/components/components.dart';
import 'package:admin_desktop/src/presentation/theme/theme.dart';
import 'package:auto_route/auto_route.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';

import 'riverpod/provider/right_side_provider.dart';

class NoteDialog extends ConsumerStatefulWidget {
  const NoteDialog({Key? key}) : super(key: key);

  @override
  ConsumerState<NoteDialog> createState() => _NoteDialogState();
}

class _NoteDialogState extends ConsumerState<NoteDialog> {
  late TextEditingController controller;

  @override
  void initState() {
    controller = TextEditingController();
    super.initState();
  }

  @override
  void didChangeDependencies() {
    controller.text = ref.watch(rightSideProvider).comment;
    super.didChangeDependencies();
  }

  @override
  void deactivate() {
    controller.dispose();
    super.deactivate();
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      width: 300.w,
      height: 240.w,
      padding: EdgeInsets.all(16.r),
      decoration: BoxDecoration(
        color: AppColors.white,
        borderRadius: BorderRadius.circular(10.r),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            AppHelpers.getTranslation(TrKeys.addComment),
            style:
                GoogleFonts.inter(fontWeight: FontWeight.w600, fontSize: 22.r),
          ),
          24.verticalSpace,
          OutlinedBorderTextField(
            textController: controller,
            label: AppHelpers.getTranslation(TrKeys.comment),
          ),
          const Spacer(),
          LoginButton(
            title: AppHelpers.getTranslation(TrKeys.save),
            onPressed: () {
              ref.read(rightSideProvider.notifier).setNote(controller.text);
              context.popRoute();
            },
          )
        ],
      ),
    );
  }
}
