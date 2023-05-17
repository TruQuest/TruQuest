import '../models/rvm/tag_vm.dart';

abstract class GeneralResultVm {}

class GetTagsSuccessVm extends GeneralResultVm {
  final List<TagVm> tags;

  GetTagsSuccessVm({required this.tags});
}
