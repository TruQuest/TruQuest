CREATE TRIGGER "OnStateChanged"
AFTER UPDATE OF "State"
ON "Things"
FOR EACH ROW
EXECUTE FUNCTION "HandleThingStateChanged"();