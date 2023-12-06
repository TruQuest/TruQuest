#!/usr/bin/env dotnet-script

#r "nuget: Nethereum.Web3, 4.11.0"
#r "nuget: Nethereum.HdWallet, 4.11.0"

using Internal;
using Nethereum.HdWallet;
using NBitcoin;

var wallet = new Wallet(Wordlist.English, WordCount.Twelve);
var account = wallet.GetAccount(0);
Console.WriteLine($"Private key: {account.PrivateKey}\nAddress: {account.Address}");
