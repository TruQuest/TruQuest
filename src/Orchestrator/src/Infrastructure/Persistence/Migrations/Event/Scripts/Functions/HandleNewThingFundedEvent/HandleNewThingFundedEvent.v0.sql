CREATE FUNCTION "HandleNewThingFundedEvent"()
    RETURNS TRIGGER
    LANGUAGE PLPGSQL
    SET search_path FROM CURRENT
AS $$
BEGIN
    PERFORM pg_notify('event_channel', CONCAT('ThingFundedEvent::', NEW."Id", '::', NEW."BlockNumber", '::', NEW."ThingIdHash"));
    RETURN NULL;
END
$$