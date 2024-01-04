import 'dart:async';
import 'package:admin_desktop/src/models/data/customer_model.dart';
import 'package:admin_desktop/src/models/data/user_data.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/customers/riverpod/state/customer_state.dart';
import 'package:admin_desktop/src/repository/repository.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../../../../core/constants/constants.dart';
import '../../../../../../../core/utils/app_connectivity.dart';
import '../../../../../../../core/utils/app_helpers.dart';
import '../../../../../../components/dialogs/successfull_dialog.dart';
import '../../../../riverpod/provider/main_provider.dart';

class CustomerNotifier extends StateNotifier<CustomerState> {
  final UsersRepository _usersRepository;
  int _page = 0;

  CustomerNotifier(
    this._usersRepository,
  ) : super(const CustomerState());

  setUser(UserData? user) {
    state = state.copyWith(selectUser: user);
  }

  
  Future<void> fetchAllUsers({
    VoidCallback? checkYourNetwork,
  }) async {
    if (!state.hasMore) {
      return;
    }
    final connected = await AppConnectivity.connectivity();
    if (connected) {
      if (_page == 0) {
        state = state.copyWith(isLoading: true, users: []);

        final response = await _usersRepository.getUsers(
          page: ++_page,
        );
        response.when(
          success: (data) {
            state = state.copyWith(
              users: data.users ?? [],
              isLoading: false,
            );
            if ((data.users?.length ?? 0) < 5) {
              state = state.copyWith(hasMore: false);
            }
          },
          failure: (failure) {
            state = state.copyWith(isLoading: false);
            debugPrint('==> get products failure: $failure');
          },
        );
      } else {
        state = state.copyWith(isMoreLoading: true);
        final response = await _usersRepository.getUsers(page: ++_page);
        response.when(
          success: (data) async {
            final List<UserData> newList = List.from(state.users);
            newList.addAll(data.users ?? []);
            state = state.copyWith(
              users: newList,
              isMoreLoading: false,
            );
            if ((data.users?.length ?? 0) < 5) {
              state = state.copyWith(hasMore: false);
            }
          },
          failure: (failure) {
            state = state.copyWith(isMoreLoading: false);
            debugPrint('==> get users  failure: $failure');
          },
        );
      }
    } else {
      checkYourNetwork?.call();
    }
  }

  Future<void> createCustomer(BuildContext context,
      {required String name,
      required String lastName,
      required String email,
      required String phone,
      required VoidCallback onSuccess}) async {
    final connected = await AppConnectivity.connectivity();
    if (connected) {
      state = state.copyWith(createUserLoading: true);

      final response = await _usersRepository.createUser(
          query: CustomerModel(
        role: 'user',
        firstname: name,
        lastname: lastName,
        email: email,
        phone: int.tryParse(phone),
      ));
      response.when(
        success: (data) {
          state = state.copyWith(
            user: data,
            createUserLoading: false,
          );
          onSuccess();
          showDialog(
              context: context,
              builder: (_) => Dialog(child: Consumer(
                    builder:
                        (BuildContext context, WidgetRef ref, Widget? child) {
                      return SuccessfullDialog(
                          title:
                              AppHelpers.getTranslation(TrKeys.customerAdded),
                          content: AppHelpers.getTranslation(TrKeys.goToHome),
                          onPressed: () {
                            Navigator.pop(context);
                            ref.read(mainProvider.notifier).changeIndex(0);
                          });
                    },
                  )));
        },
        failure: (failure) {
          state = state.copyWith(createUserLoading: false);
          AppHelpers.showSnackBar(
            context,
            AppHelpers.getTranslation(failure.toString()),
          );
        },
      );
    } else {
      if (mounted) {
        AppHelpers.showSnackBar(
          context,
          AppHelpers.getTranslation(TrKeys.checkYourNetworkConnection),
        );
      }
    }
  }

  Future<void> searchUsers(BuildContext context, String text) async {
    final connected = await AppConnectivity.connectivity();
    if (connected) {
      if (state.query == text) {
        return;
      }
      state = state.copyWith(isLoading: true, query: text);
      final response = await _usersRepository.searchUsers(text.trim());
      response.when(
        success: (data) async {
          state = state.copyWith(isLoading: false, users: data.users ?? []);
        },
        failure: (activeFailure) {
          state = state.copyWith(isLoading: false);
          AppHelpers.showFlashBar(
            context,
            AppHelpers.getTranslation(activeFailure.toString()),
          );
        },
      );
    } else {
      if (mounted) {
        AppHelpers.showFlashBar(context, mounted.toString());
      }
    }
  }
}
