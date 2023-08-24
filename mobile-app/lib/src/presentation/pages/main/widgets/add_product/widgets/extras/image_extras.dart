import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';

import '../../../../../../../models/models.dart';
import '../../../../../../components/components.dart';
import '../../../../../../theme/theme.dart';
import '../../../right_side/riverpod/provider/right_side_provider.dart';
import '../../provider/add_product_provider.dart';

class ImageExtras extends ConsumerWidget {
  final int groupIndex;
  final List<UiExtra> uiExtras;

  const ImageExtras({
    Key? key,
    required this.groupIndex,
    required this.uiExtras,
  }) : super(key: key);

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final notifier = ref.read(addProductProvider.notifier);
    final rightSideState = ref.watch(rightSideProvider);
    return Wrap(
      spacing: 8.r,
      runSpacing: 10.r,
      children: uiExtras
          .map(
            (uiExtra) => Material(
              borderRadius: BorderRadius.circular(21.r),
              child: InkWell(
                borderRadius: BorderRadius.circular(21.r),
                onTap: () {
                  if (uiExtra.isSelected) {
                    return;
                  }
                  notifier.updateSelectedIndexes(
                    index: groupIndex,
                    value: uiExtra.index,
                    bagIndex: rightSideState.selectedBagIndex,
                  );
                },
                child: Container(
                  width: 42.r,
                  height: 42.r,
                  decoration: BoxDecoration(
                    borderRadius: BorderRadius.circular(21.r),
                  ),
                  child: Stack(
                    children: [
                      CommonImage(
                        imageUrl: uiExtra.value,
                        width: 42,
                        height: 42,
                        radius: 21,
                      ),
                      if (uiExtra.isSelected)
                        Align(
                          alignment: Alignment.center,
                          child: Container(
                            width: 22.r,
                            height: 22.r,
                            decoration: BoxDecoration(
                              borderRadius: BorderRadius.circular(11.r),
                              color: AppColors.brandColor,
                              border: Border.all(
                                color: AppColors.white,
                                width: 8.r,
                              ),
                            ),
                          ),
                        ),
                    ],
                  ),
                ),
              ),
            ),
          )
          .toList(),
    );
  }
}
