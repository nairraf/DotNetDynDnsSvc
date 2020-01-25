CREATE TABLE "users" (
	"username"	TEXT NOT NULL,
	"key"	TEXT NOT NULL,
	"resourceRecord"	TEXT,
	"zone"	TEXT,
	"allowedActions"	TEXT NOT NULL,
	PRIMARY KEY("username","key")
)