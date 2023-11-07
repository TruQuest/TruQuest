CREATE FUNCTION "AddUserIdToEvent"()
    RETURNS TRIGGER
    LANGUAGE PLPGSQL
AS $$
DECLARE
    userId TEXT;
BEGIN
    SELECT u."Id" INTO userId
    FROM truquest."AspNetUsers" AS u
    WHERE u."WalletAddress" = NEW."WalletAddress";

    IF FOUND THEN
        NEW."UserId" = userId;
    END IF;

    RETURN NEW;
END
$$
