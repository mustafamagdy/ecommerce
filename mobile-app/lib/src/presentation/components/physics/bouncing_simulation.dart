import 'dart:math' as math;
import 'package:flutter/physics.dart';
import 'package:flutter/foundation.dart';

class CustomBouncingScrollSimulation extends Simulation {
  CustomBouncingScrollSimulation({
    required double position,
    required double velocity,
    required this.leadingExtent,
    required this.trailingExtent,
    required this.spring,
    Tolerance tolerance = Tolerance.defaultTolerance,
  })  : assert(leadingExtent <= trailingExtent),
        super(tolerance: tolerance) {
    if (position < leadingExtent) {
      _springSimulation = _underScrollSimulation(
        position,
        (velocity.abs() > 1000) ? (velocity.sign * 1000) : velocity,
      );
      _springTime = double.negativeInfinity;
    } else if (position > trailingExtent) {
      _springSimulation = _overscrollSimulation(
        position,
        (velocity.abs() > 1000) ? (velocity.sign * 1000) : velocity,
      );
      _springTime = double.negativeInfinity;
    } else {
      _frictionSimulation = FrictionSimulation(0.135, position, velocity);
      final double finalX = _frictionSimulation.finalX;
      if (velocity > 0.0 && finalX > trailingExtent) {
        _springTime = _frictionSimulation.timeAtX(trailingExtent);
        _springSimulation = _overscrollSimulation(
          trailingExtent,
          math.min(
              _frictionSimulation.dx(_springTime), maxSpringTransferVelocity),
        );
        assert(_springTime.isFinite);
      } else if (velocity < 0.0 && finalX < leadingExtent) {
        _springTime = _frictionSimulation.timeAtX(leadingExtent);
        _springSimulation = _underScrollSimulation(
          leadingExtent,
          math.min(
              _frictionSimulation.dx(_springTime), maxSpringTransferVelocity),
        );
        assert(_springTime.isFinite);
      } else {
        _springTime = double.infinity;
      }
    }
  }

  static const double maxSpringTransferVelocity = 5000.0;

  final double leadingExtent;

  final double trailingExtent;

  final SpringDescription spring;

  late FrictionSimulation _frictionSimulation;
  late Simulation _springSimulation;
  late double _springTime;
  double _timeOffset = 0.0;

  Simulation _underScrollSimulation(double x, double dx) {
    return ScrollSpringSimulation(spring, x, leadingExtent, dx);
  }

  Simulation _overscrollSimulation(double x, double dx) {
    return ScrollSpringSimulation(spring, x, trailingExtent, dx);
  }

  Simulation _simulation(double time) {
    final Simulation simulation;
    if (time > _springTime) {
      _timeOffset = _springTime.isFinite ? _springTime : 0.0;
      simulation = _springSimulation;
    } else {
      _timeOffset = 0.0;
      simulation = _frictionSimulation;
    }
    return simulation..tolerance = tolerance;
  }

  @override
  double x(double time) => _simulation(time).x(time - _timeOffset);

  @override
  double dx(double time) => _simulation(time).dx(time - _timeOffset);

  @override
  bool isDone(double time) => _simulation(time).isDone(time - _timeOffset);

  @override
  String toString() {
    return '${objectRuntimeType(this, 'BouncingScrollSimulation')}(leadingExtent: $leadingExtent, trailingExtent: $trailingExtent)';
  }
}
