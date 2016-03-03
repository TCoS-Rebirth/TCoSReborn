GameServer and LoginServer both require a MySql server (tested with v5.3)
"Spellborn_TableSetup.sql" can be run/imported to create the required tables and perform the initial setup.
The "server" table should not be edited!
To elevate an account, set the accountlevel in the "accounts" table to something higher than 1
(0 = player, 1 = client crash! dont use!, 2 = gm, >2 = Admin)