import 'package:admin_desktop/src/presentation/pages/main/riverpod/provider/main_provider.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';

import 'left_side.dart';
import 'order_calculate/order_calculate.dart';
import 'right_side/right_side.dart';

class PostPage extends ConsumerWidget {
  const PostPage({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context,ref) {
    return ref.watch(mainProvider).priceDate != null
        ? const OrderCalculate()
        :  Padding(
          padding: REdgeInsets.symmetric(horizontal: 15),
          child: Column(
            children: [
              15.verticalSpace,
              Expanded(
                child: Row(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Expanded(child: LeftSide()),
                    16.horizontalSpace,
                    SizedBox(width: MediaQuery.of(context).size.width/3.2, child: const RightSide()),
                  ],
                ),
              ),
            ],
          ),
        );
  }
}
