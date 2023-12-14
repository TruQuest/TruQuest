import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

enum TokenUnit {
  tru,
  gt,
}

class TokenUnitSwitcher extends StatefulWidget {
  final void Function(TokenUnit selectedUnit) onUnitSwitched;

  const TokenUnitSwitcher({super.key, required this.onUnitSwitched});

  @override
  State<TokenUnitSwitcher> createState() => _TokenUnitSwitcherState();
}

class _TokenUnitSwitcherState extends State<TokenUnitSwitcher> {
  late TokenUnit _selectedUnit;

  @override
  void initState() {
    super.initState();
    _selectedUnit = TokenUnit.tru;
    widget.onUnitSwitched(_selectedUnit);
  }

  Widget _buildButton(TokenUnit unit) {
    return ElevatedButton(
      style: ElevatedButton.styleFrom(
        backgroundColor: _selectedUnit == unit ? const Color(0xffF8F9FA) : const Color.fromARGB(255, 161, 161, 162),
        foregroundColor: _selectedUnit == unit ? const Color(0xFF242423) : const Color.fromARGB(255, 66, 66, 65),
        elevation: 5,
        padding: EdgeInsets.zero,
        minimumSize: const Size(40, 30),
        maximumSize: const Size(40, 30),
      ),
      child: Text(
        unit.toString().split('.').last.toUpperCase(),
        style: GoogleFonts.righteous(fontSize: 11),
      ),
      onPressed: () {
        if (_selectedUnit != unit) {
          setState(() => _selectedUnit = unit);
          widget.onUnitSwitched(unit);
        }
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        _buildButton(TokenUnit.tru),
        _buildButton(TokenUnit.gt),
      ],
    );
  }
}
