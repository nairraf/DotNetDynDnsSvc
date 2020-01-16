CREATE TABLE "users" (
	"username"	TEXT NOT NULL,
	"key"	TEXT NOT NULL,
	"resourceRecord"	TEXT NOT NULL,
	"zone"	TEXT NOT NULL,
	PRIMARY KEY("username","key")
)