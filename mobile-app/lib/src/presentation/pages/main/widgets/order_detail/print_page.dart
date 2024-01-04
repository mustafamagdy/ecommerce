// ignore_for_file: depend_on_referenced_packages

import 'dart:async';
import 'dart:developer';

import 'package:admin_desktop/src/models/data/addons_data.dart';
import 'package:admin_desktop/src/presentation/pages/main/widgets/order_detail/printer/bluetooth_printer.dart';
import 'package:flutter/material.dart';
import 'package:flutter_pos_printer_platform/esc_pos_utils_platform/esc_pos_utils_platform.dart';
import 'dart:io';

import 'package:flutter_pos_printer_platform/flutter_pos_printer_platform.dart';
import 'package:intl/intl.dart';

import '../../../../../core/constants/constants.dart';
import '../../../../../core/utils/utils.dart';
import '../../../../../models/data/order_data.dart';
import '../../../../components/components.dart';

class PrintPage extends StatefulWidget {
  final OrderData? orderData;

  const PrintPage({Key? key, required this.orderData}) : super(key: key);

  @override
  State<PrintPage> createState() => _PrintPageState();
}

class _PrintPageState extends State<PrintPage> {
  // Printer Type [bluetooth, usb, network]
  var defaultPrinterType = PrinterType.bluetooth;
  var _isBle = false;
  String customDivider = "-------------------------------------------";
  String customLine =    "___________________________________________";
  String customSpace = "  ";
  var _reconnect = false;
  var printerManager = PrinterManager.instance;
  var devices = <BluetoothPrinter>[];
  StreamSubscription<PrinterDevice>? _subscription;
  StreamSubscription<BTStatus>? _subscriptionBtStatus;
  StreamSubscription<USBStatus>? _subscriptionUsbStatus;
  StreamSubscription<TCPStatus>? _subscriptionTCPStatus;
  BTStatus _currentStatus = BTStatus.none;

  // ignore: unused_field
  TCPStatus _currentTCPStatus = TCPStatus.none;

  // _currentUsbStatus is only supports on Android
  // ignore: unused_field
  USBStatus _currentUsbStatus = USBStatus.none;
  List<int>? pendingTask;
  String _ipAddress = '';
  String _port = '9100';
  final _ipController = TextEditingController();
  final _portController = TextEditingController();
  BluetoothPrinter? selectedPrinter;

  @override
  void initState() {
    if (Platform.isWindows) defaultPrinterType = PrinterType.usb;
    super.initState();
    _portController.text = _port;
    _scan();

    // subscription to listen change status of bluetooth connection
    _subscriptionBtStatus =
        PrinterManager.instance.stateBluetooth.listen((status) {
      log(' ----------------- status bt $status ------------------ ');
      _currentStatus = status;

      if (status == BTStatus.connected && pendingTask != null) {
        if (Platform.isAndroid) {
          Future.delayed(const Duration(milliseconds: 1000), () {
            PrinterManager.instance
                .send(type: PrinterType.bluetooth, bytes: pendingTask!);
            pendingTask = null;
          });
        } else if (Platform.isIOS) {
          PrinterManager.instance
              .send(type: PrinterType.bluetooth, bytes: pendingTask!);
          pendingTask = null;
        }
      }
    });
    //  PrinterManager.instance.stateUSB is only supports on Android
    _subscriptionUsbStatus = PrinterManager.instance.stateUSB.listen((status) {
      log(' ----------------- status usb $status ------------------ ');
      _currentUsbStatus = status;
      if (Platform.isAndroid) {
        if (status == USBStatus.connected && pendingTask != null) {
          Future.delayed(const Duration(milliseconds: 1000), () {
            PrinterManager.instance
                .send(type: PrinterType.usb, bytes: pendingTask!);
            pendingTask = null;
          });
        }
      }
    });

    //  PrinterManager.instance.stateUSB is only supports on Android
    _subscriptionTCPStatus = PrinterManager.instance.stateTCP.listen((status) {
      log(' ----------------- status tcp $status ------------------ ');
      _currentTCPStatus = status;
    });
  }

  @override
  void dispose() {
    _subscription?.cancel();
    _subscriptionBtStatus?.cancel();
    _subscriptionUsbStatus?.cancel();
    _subscriptionTCPStatus?.cancel();
    _portController.dispose();
    _ipController.dispose();
    super.dispose();
  }

  // method to scan devices according PrinterType
  void _scan() {
    devices.clear();
    _subscription = printerManager
        .discovery(type: defaultPrinterType, isBle: _isBle)
        .listen((device) {
      devices.add(BluetoothPrinter(
        deviceName: device.name,
        address: device.address,
        isBle: _isBle,
        vendorId: device.vendorId,
        productId: device.productId,
        typePrinter: defaultPrinterType,
      ));
      setState(() {});
    });
  }

  void setPort(String value) {
    if (value.isEmpty) value = '9100';
    _port = value;
    var device = BluetoothPrinter(
      deviceName: value,
      address: _ipAddress,
      port: _port,
      typePrinter: PrinterType.network,
      state: false,
    );
    selectDevice(device);
  }

  void setIpAddress(String value) {
    _ipAddress = value;
    var device = BluetoothPrinter(
      deviceName: value,
      address: _ipAddress,
      port: _port,
      typePrinter: PrinterType.network,
      state: false,
    );
    selectDevice(device);
  }

  void selectDevice(BluetoothPrinter device) async {
    if (selectedPrinter != null) {
      if ((device.address != selectedPrinter!.address) ||
          (device.typePrinter == PrinterType.usb &&
              selectedPrinter!.vendorId != device.vendorId)) {
        await PrinterManager.instance
            .disconnect(type: selectedPrinter!.typePrinter);
      }
    }

    selectedPrinter = device;
    setState(() {});
  }

  Future _printReceiveTest() async {
    List<int> bytes = [];
    num subTotal = 0;
    subTotal = ((widget.orderData?.totalPrice ?? 0) -
        (widget.orderData?.tax ?? 0) -
        (widget.orderData?.deliveryFee ?? 0) +
        (widget.orderData?.totalDiscount ?? 0));

    // Xprinter XP-N160I
    final profile = await CapabilityProfile.load(name: 'XP-N160I');

    // PaperSize.mm80 or PaperSize.mm58
    final generator = Generator(PaperSize.mm80, profile);
    bytes += generator.setGlobalCodeTable('CP1252');
    bytes += generator.setStyles(const PosStyles(align: PosAlign.center));
    bytes += generator.text(AppHelpers.getTranslation(TrKeys.orderSummary),
        styles: const PosStyles(align: PosAlign.center, bold: true));

    bytes += generator.text(
        "${AppHelpers.getTranslation(TrKeys.order)} #${AppHelpers.getTranslation(TrKeys.id)}${widget.orderData?.id}",
        styles: const PosStyles(align: PosAlign.center));
    bytes+=generator.text(customDivider);
    bytes += generator.row([
      PosColumn(
          width: 7,
          text: customSpace+AppHelpers.getTranslation(TrKeys.shopName),
          styles: const PosStyles(align: PosAlign.left, codeTable: 'CP1252')),
      PosColumn(
          width: 5,
          text: widget.orderData?.shop?.translation?.title ?? "",
          styles: const PosStyles(align: PosAlign.right, codeTable: 'CP1252')),
    ]);
    bytes += generator.row([
      PosColumn(
        width: 6,
        text: customSpace+ AppHelpers.getTranslation(TrKeys.client),
        styles: const PosStyles(align: PosAlign.left),
      ),
      PosColumn(
        width: 6,
        text:
            "${widget.orderData?.user?.firstname ?? ""} ${widget.orderData?.user?.lastname ?? ""}",
        styles: const PosStyles(align: PosAlign.right),
      ),
    ]);
    bytes += generator.row([
      PosColumn(
        width: 6,
        text:customSpace+ AppHelpers.getTranslation(TrKeys.date),
        styles: const PosStyles(align: PosAlign.left),
      ),
      PosColumn(
        width: 6,
        text: DateFormat("MM/dd/yy HH:mm").format(DateTime.tryParse(widget.orderData?.createdAt ?? "") ?? DateTime.now()),
        styles: const PosStyles(align: PosAlign.right),
      ),
    ]);

    bytes+=generator.text(customLine);
    for (int index = 0;
        index < (widget.orderData?.details?.length ?? 0);
        index++) {
      bytes += generator.setStyles(const PosStyles(align: PosAlign.center));
      bytes += generator.row([
        PosColumn(
          width: 8,
          text:
          "$customSpace${widget.orderData?.details?[index].stock?.product?.translation?.title ?? ""} x ${widget.orderData?.details?[index].quantity ?? ""}",
          styles: const PosStyles(align: PosAlign.left,bold: true),
        ),
        PosColumn(
          width: 4,
          text: NumberFormat.currency(
            symbol: widget.orderData?.currency?.symbol,
          ).format(widget.orderData?.details?[index].totalPrice ?? 0),
          styles: const PosStyles(align: PosAlign.right,bold: true),
        ),
       
      ]);
      for (int i = 0;
      i < (widget.orderData?.details?[index].addons?.length ?? 0);
      i++)
      {
        Addons addons = widget.orderData!.details![index].addons![i];
        bytes += generator.row([
          PosColumn(
            width: 10,
            text:  "$customSpace${addons.stocks?.product?.translation?.title ?? ""} ( ${NumberFormat.currency(
              symbol: widget.orderData?.currency?.symbol ?? "",
            ).format((addons.price ?? 0) / (addons.quantity ?? 1))} x ${(addons.quantity ?? 1)} )",

            styles: const PosStyles(align: PosAlign.left),
          ),
          PosColumn(
            width: 2,
            text: "",
            styles: const PosStyles(align: PosAlign.right),
          ),
        ]);
      }
      bytes += generator.setStyles(const PosStyles(align: PosAlign.center));
      bytes+=generator.text(customDivider);

    }
    bytes+=generator.text(customDivider);
    bytes += generator.row([
      PosColumn(
        width: 6,
        text:
        customSpace+ AppHelpers.getTranslation(TrKeys.subtotal),
        styles: const PosStyles(align: PosAlign.left,bold: true),
      ),
      PosColumn(
        width: 6,
        text:
        NumberFormat.currency(
          symbol: LocalStorage.instance
              .getSelectedCurrency()
              .symbol,
        ).format(subTotal),
        styles: const PosStyles(align: PosAlign.right,bold: true),
      ),

    ]);
    bytes += generator.row([
      PosColumn(
        width: 6,
        text:
        customSpace+ AppHelpers.getTranslation(TrKeys.tax),
        styles: const PosStyles(align: PosAlign.left,bold: true),
      ),
      PosColumn(
        width: 6,
        text:
        NumberFormat.currency(
          symbol: LocalStorage.instance
              .getSelectedCurrency()
              .symbol,
        ).format(widget.orderData?.tax ?? 0),
        styles: const PosStyles(align: PosAlign.right,bold: true),
      ),

    ]);

    bytes += generator.row([
      PosColumn(
        width: 6,
        text:
        customSpace+ AppHelpers.getTranslation(TrKeys.deliveryFee),
        styles: const PosStyles(align: PosAlign.left,bold: true),
      ),
      PosColumn(
        width: 6,
        text:
        NumberFormat.currency(
          symbol: LocalStorage.instance
              .getSelectedCurrency()
              .symbol,
        ).format(widget.orderData?.deliveryFee ?? 0),
        styles: const PosStyles(align: PosAlign.right,bold: true),
      ),

    ]);
    bytes += generator.row([
      PosColumn(
        width: 6,
        text:
        customSpace+   AppHelpers.getTranslation(TrKeys.discount),
        styles: const PosStyles(align: PosAlign.left,bold: true),
      ),
      PosColumn(
        width: 6,
        text:
        NumberFormat.currency(
          symbol: LocalStorage.instance
              .getSelectedCurrency()
              .symbol,
        ).format(widget.orderData?.totalDiscount ?? 0),
        styles: const PosStyles(align: PosAlign.right,bold: true),
      ),

    ]);
    bytes += generator.row([
      PosColumn(
        width: 6,
        text:
        customSpace+ AppHelpers.getTranslation(TrKeys.totalPrice),
        styles: const PosStyles(align: PosAlign.left,bold: true),
      ),
      PosColumn(
        width: 6,
        text:
        NumberFormat.currency(
          symbol: LocalStorage.instance
              .getSelectedCurrency()
              .symbol,
        ).format(widget.orderData?.totalPrice ?? 0),

        styles: const PosStyles(align: PosAlign.right,bold: true),
      ),

    ]);
    bytes+=generator.text(customDivider);
    bytes+=generator.text(AppHelpers.getTranslation(TrKeys.thankYou));


    _printEscPos(bytes, generator);
  }

  /// print ticket
  void _printEscPos(List<int> bytes, Generator generator) async {
    var connectedTCP = false;
    if (selectedPrinter == null) return;
    var bluetoothPrinter = selectedPrinter!;

    switch (bluetoothPrinter.typePrinter) {
      case PrinterType.usb:
        debugPrint("");
        bytes += generator.feed(2);
        bytes += generator.cut();
        await printerManager.connect(
            type: bluetoothPrinter.typePrinter,
            model: UsbPrinterInput(
                name: bluetoothPrinter.deviceName,
                productId: bluetoothPrinter.productId,
                vendorId: bluetoothPrinter.vendorId));
        pendingTask = null;
        break;
      case PrinterType.bluetooth:
        bytes += generator.cut();
        await printerManager.connect(
            type: bluetoothPrinter.typePrinter,
            model: BluetoothPrinterInput(
                name: bluetoothPrinter.deviceName,
                address: bluetoothPrinter.address!,
                isBle: bluetoothPrinter.isBle ?? false,
                autoConnect: _reconnect));
        pendingTask = null;
        if (Platform.isAndroid) pendingTask = bytes;
        break;
      case PrinterType.network:
        bytes += generator.feed(2);
        bytes += generator.cut();
        connectedTCP = await printerManager.connect(
            type: bluetoothPrinter.typePrinter,
            model: TcpPrinterInput(ipAddress: bluetoothPrinter.address!));
        if (!connectedTCP) debugPrint(' --- please review your connection ---');
        break;
      default:
    }
    if (bluetoothPrinter.typePrinter == PrinterType.bluetooth &&
        Platform.isAndroid) {
      if (_currentStatus == BTStatus.connected) {
        printerManager.send(type: bluetoothPrinter.typePrinter, bytes: bytes);
        pendingTask = null;
      }
    } else {
      printerManager.send(type: bluetoothPrinter.typePrinter, bytes: bytes);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      constraints: const BoxConstraints(maxWidth: 400),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          DropdownButtonFormField<PrinterType>(
            value: defaultPrinterType,
            decoration: const InputDecoration(
              prefixIcon: Icon(Icons.print, size: 24),
              labelText: "Type Printer Device",
              labelStyle: TextStyle(fontSize: 18.0),
              focusedBorder: InputBorder.none,
              enabledBorder: InputBorder.none,
            ),
            items: <DropdownMenuItem<PrinterType>>[
              if (Platform.isAndroid || Platform.isIOS)
                const DropdownMenuItem(
                  value: PrinterType.bluetooth,
                  child: Text("bluetooth"),
                ),
              if (Platform.isAndroid || Platform.isWindows)
                const DropdownMenuItem(
                  value: PrinterType.usb,
                  child: Text("usb"),
                ),
              const DropdownMenuItem(
                value: PrinterType.network,
                child: Text("Wifi"),
              ),
            ],
            onChanged: (PrinterType? value) {
              setState(() {
                if (value != null) {
                  setState(() {
                    defaultPrinterType = value;
                    selectedPrinter = null;
                    _isBle = false;

                    _scan();
                  });
                }
              });
            },
          ),
          Visibility(
            visible: defaultPrinterType == PrinterType.bluetooth &&
                Platform.isAndroid,
            child: SwitchListTile.adaptive(
              contentPadding: const EdgeInsets.only(bottom: 20.0, left: 20),
              title: const Text(
                "This device supports ble (low energy)",
                textAlign: TextAlign.start,
                style: TextStyle(fontSize: 19.0),
              ),
              value: _isBle,
              onChanged: (bool? value) {
                setState(() {
                  _isBle = value ?? false;

                  selectedPrinter = null;
                  _scan();
                });
              },
            ),
          ),
          Visibility(
            visible: defaultPrinterType == PrinterType.bluetooth &&
                Platform.isAndroid,
            child: SwitchListTile.adaptive(
              contentPadding: const EdgeInsets.only(bottom: 20.0, left: 20),
              title: const Text(
                "reconnect",
                textAlign: TextAlign.start,
                style: TextStyle(fontSize: 19.0),
              ),
              value: _reconnect,
              onChanged: (bool? value) {
                setState(() {
                  _reconnect = value ?? false;
                });
              },
            ),
          ),
          Column(
              children: devices
                  .map(
                    (device) => ListTile(
                      title: Text('${device.deviceName}'),
                      subtitle: Platform.isAndroid &&
                              defaultPrinterType == PrinterType.usb
                          ? null
                          : Visibility(
                              visible: !Platform.isWindows,
                              child: Text("${device.address}")),
                      onTap: () => selectDevice(device),
                      leading: selectedPrinter != null &&
                              ((device.typePrinter == PrinterType.usb &&
                                          Platform.isWindows
                                      ? device.deviceName ==
                                          selectedPrinter!.deviceName
                                      : device.vendorId != null &&
                                          selectedPrinter!.vendorId ==
                                              device.vendorId) ||
                                  (device.address != null &&
                                      selectedPrinter!.address ==
                                          device.address))
                          ? const Icon(
                              Icons.check,
                              color: Colors.green,
                            )
                          : null,
                      trailing: OutlinedButton(
                        onPressed: selectedPrinter == null ||
                                device.deviceName != selectedPrinter?.deviceName
                            ? null
                            : () async {
                                _printReceiveTest();
                              },
                        child: const Padding(
                          padding:
                              EdgeInsets.symmetric(vertical: 2, horizontal: 20),
                          child: Text("Print test ticket",
                              textAlign: TextAlign.center),
                        ),
                      ),
                    ),
                  )
                  .toList()),
          Visibility(
            visible:
                defaultPrinterType == PrinterType.network && Platform.isWindows,
            child: Padding(
              padding: const EdgeInsets.only(top: 10.0),
              child: TextFormField(
                controller: _ipController,
                keyboardType:
                    const TextInputType.numberWithOptions(signed: true),
                decoration: const InputDecoration(
                  label: Text("Ip Address"),
                  prefixIcon: Icon(Icons.wifi, size: 24),
                ),
                onChanged: setIpAddress,
              ),
            ),
          ),
          Visibility(
            visible:
                defaultPrinterType == PrinterType.network && Platform.isWindows,
            child: Padding(
              padding: const EdgeInsets.only(top: 10.0),
              child: TextFormField(
                controller: _portController,
                keyboardType:
                    const TextInputType.numberWithOptions(signed: true),
                decoration: const InputDecoration(
                  label: Text("Port"),
                  prefixIcon: Icon(Icons.numbers_outlined, size: 24),
                ),
                onChanged: setPort,
              ),
            ),
          ),
          Visibility(
            visible:
                defaultPrinterType == PrinterType.network && Platform.isWindows,
            child: Padding(
              padding: const EdgeInsets.only(top: 10.0),
              child: ConfirmButton(
                paddingSize: 32,
                onTap: () async {
                  _printReceiveTest();
                },
                title: "Print",
              ),
            ),
          )
        ],
      ),
    );
  }
}
