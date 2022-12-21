CREATE FUNCTION "SelectWinnerIndicesAccordingToPreJoinedThingSubmissionVerifierLotteryEvents"("ThingId" UUID, "WinnerIds" TEXT[])
    RETURNS TABLE (
        "UserId" TEXT,
        "Index" BIGINT
    )
    LANGUAGE PLPGSQL
    SET search_path FROM CURRENT
AS $$
BEGIN
    RETURN QUERY
    	WITH "UserIdAndRowNumber" AS (
	        SELECT
	            e."UserId",
	            ROW_NUMBER() OVER (ORDER BY e."BlockNumber", e."TxnIndex") AS "RowNumber"
	        FROM "PreJoinedThingSubmissionVerifierLotteryEvents" AS e
	        WHERE e."ThingId" = $1
	    )
	    SELECT val."UserId", val."RowNumber" - 1
	    FROM "UserIdAndRowNumber" AS val
	    WHERE val."UserId" = ANY($2)
	    ORDER BY val."RowNumber";
END
$$