abstract class ThingResultVm {}

abstract class CreateNewThingDraftResultVm extends ThingResultVm {}

class CreateNewThingDraftSuccessVm extends CreateNewThingDraftResultVm {}

class CreateNewThingDraftFailureVm extends CreateNewThingDraftResultVm {}

class SubmitNewThingSuccessVm extends ThingResultVm {}
