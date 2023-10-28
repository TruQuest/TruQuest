CREATE FUNCTION "AddUserIdFromWalletAddress"()
    RETURNS TRIGGER
    LANGUAGE PLPGSQL
AS $$
DECLARE
    userId TEXT;
BEGIN
    SELECT c."UserId" INTO userId
    FROM truquest."AspNetUserClaims" AS c
    WHERE c."ClaimType" = 'wallet_address' AND c."ClaimValue" = NEW."WalletAddress";

    IF FOUND THEN
        NEW."UserId" = userId;
    END IF;

    RETURN NEW;
END
$$
