
import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:google_fonts/google_fonts.dart';

class AppbarItem extends StatelessWidget {
  final String title;
  final IconData icon;
  final String desc;

  const AppbarItem(
      {Key? key, required this.title, required this.icon, required this.desc})
      : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Icon(
          icon,
          size: 36.r,
        ),
        8.horizontalSpace,
        Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              title,
              style: GoogleFonts.inter(fontSize: 20.r),
            ),
            Text(
              desc,
              style: GoogleFonts.inter(
                  fontSize: 24.r, fontWeight: FontWeight.w700),
            )
          ],
        )
      ],
    );
  }
}
