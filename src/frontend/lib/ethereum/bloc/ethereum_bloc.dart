import "ethereum_actions.dart";
import "ethereum_result_vm.dart";
import "../services/ethereum_service.dart";
import "../../general/bloc/bloc.dart";

class EthereumBloc extends Bloc<EthereumAction> {
  final EthereumService _ethereumService;

  EthereumBloc(this._ethereumService) {
    actionChannel.stream.listen((action) {
      if (action is ConnectEthereumAccount) {
        _connectEthereumAccount(action);
      } else if (action is SignAuthMessage) {
        _signAuthMessage(action);
      }
    });
  }

  @override
  void dispose({EthereumAction? cleanupAction}) {}

  void _connectEthereumAccount(ConnectEthereumAccount action) async {
    var error = await _ethereumService.connectAccount();
    action.complete(error != null ? ConnectEthereumAccountFailureVm() : null);
  }

  void _signAuthMessage(SignAuthMessage action) async {
    var result = await _ethereumService.signAuthMessage(action.username);
    if (result.isLeft) {
      action.complete(SignAuthMessageFailureVm());
      return;
    }

    var signature = result.right;
    action.complete(SignAuthMessageSuccessVm(signature: signature));
  }
}
