CREATE FUNCTION "HandleThingStateChanged"()
    RETURNS TRIGGER
    LANGUAGE PLPGSQL
    SET search_path FROM CURRENT
AS $$
BEGIN
    PERFORM pg_notify('event_channel', CONCAT('Thing::', NEW."Id", '::', NEW."State"));
    RETURN NULL;
END
$$