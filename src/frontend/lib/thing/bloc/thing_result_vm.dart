abstract class ThingResultVm {}

abstract class CreateNewThingDraftResultVm extends ThingResultVm {}

class CreateNewThingDraftSuccessVm extends CreateNewThingDraftResultVm {
  final Stream<int> progress$;

  CreateNewThingDraftSuccessVm({required this.progress$});
}

class CreateNewThingDraftFailureVm extends CreateNewThingDraftResultVm {}
