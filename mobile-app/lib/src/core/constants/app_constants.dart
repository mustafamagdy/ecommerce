class AppConstants {
  AppConstants._();

  /// shared preferences keys
  static const String keyLangSelected = 'keyLangSelected';
  static const String keyLanguageData = 'keyLanguageData';
  static const String keyToken = 'keyToken';
  static const String keyGlobalSettings = 'keyGlobalSettings';
  static const String keyActiveLocale = 'keyActiveLocale';
  static const String keyTranslations = 'keyTranslations';
  static const String keySelectedCurrency = 'keySelectedCurrency';
  static const String keyBags = 'keyBags';
  static const String keyUser = 'keyUser';
  static const String keyTenant = 'keyTenant';
  static const String pinCode = 'pinCode';
  static const String demoSellerLogin = 'sellers@githubit.com';
  static const String demoSellerPassword = 'seller';
  static const String demoCookerLogin = 'cook@githubit.com';
  static const String demoCookerPassword = 'cook';
  static const Duration refreshTime = Duration(seconds: 30);
  static const double demoLatitude = 41.304223;
  static const double demoLongitude = 69.2348277;

  static const double pinLoadingMin = 0.116666667;
  static const double pinLoadingMax = 0.611111111;
}

enum DropDownType {  categories, users }

enum ExtrasType { color, text, image }

enum ChairPosition { top, bottom, left,right }


enum UploadType {
  extras,
  brands,
  categories,
  shopsLogo,
  shopsBack,
  products,
  reviews,
  users,
}



enum OrderStatus {
  newOrder,
  accepted,
  cooking,
  ready,
  onAWay,
  delivered,
  canceled,
}