
import 'package:admin_desktop/src/core/di/dependency_manager.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../notifier/edit_profile_notifier.dart';
import '../state/edit_profile_state.dart';

final editProfileProvider =
    StateNotifierProvider.autoDispose<EditProfileNotifier, EditProfileState>(
  (ref) => EditProfileNotifier(galleryRepository),
);
