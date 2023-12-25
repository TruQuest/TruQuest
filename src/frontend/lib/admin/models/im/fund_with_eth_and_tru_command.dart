class FundWithEthAndTruCommand {
  final double amountInEth;
  final double amountInTru;

  FundWithEthAndTruCommand({
    required this.amountInEth,
    required this.amountInTru,
  });

  Map<String, dynamic> toJson() => {
        'amountInEth': amountInEth,
        'amountInTru': amountInTru,
      };
}
