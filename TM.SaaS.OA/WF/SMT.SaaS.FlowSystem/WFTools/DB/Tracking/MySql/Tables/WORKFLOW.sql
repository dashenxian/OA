DROP TABLE IF EXISTS WORKFLOW;
CREATE TABLE WORKFLOW
(
	WORKFLOW_TYPE_ID BIGINT UNSIGNED NOT NULL
	,WORKFLOW_DEFINITION LONGTEXT NULL
	,PRIMARY KEY ( WORKFLOW_TYPE_ID )
);
