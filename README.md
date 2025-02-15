# slowvault
Store secrets with a delay on retrieval.

Use this to protect passwords from yourself. Fight addiction by using your own blocking tools, protect unblocking with a password and store said password in here.
Essentially it's just a fancy key value store. Do not use this as a password manager.

---

This project is incomplete.

---

Specs:

CLI:
slowvault create [filepath]: Create a new vault at the given location, optionally protected by a password (this will encrypt the file with that password, recommended, hidden from console)
slowvault add [key] [options]: Add a new key to the vault, will prompt the value afterwards (hidden from console, you can add the value immediately using the --value option)
slowvault fetch [key]: Unlock a key to be copied to the clipboard, will ask for the password. Will ask if you want to copy or print once unlocking is complete
>> Do you wish to Copy, Print or lock the vault again? (C/p/l):
slowvault delete [key]: Delete a key from the vault, needs to be unlocked
slowvault view [key]: View the options set for a key, can also be used to check if a key exists
slowvault list: List all the keys stored in the vault.

Options:
--delay, -d: Set the unlock delay in seconds, defaults to 300 (5 minutes)
--available, -a: Set how long a key remains unlocked after the delay has passed, defaults to 300 (5 minutes)
--lock-on-copy, -l: Whether to lock the key after it has been copied or printed, defaults to true
--clear-clipboard, -c: Set how long after a value is copied to the clipboard that the clipboard needs to be cleared, defaults to 60 (1 minute)
--prompt-again, -p: Prompt for password again when copying or printing, defaults to false. Will not ask this when deleting a key.
All these options are defined on the "add" function, none of the other functions have options

--file, -f: Filepath of the vault, may also be defined as environment variable SLOWVAULT_PATH
Either --file or SLOWVAULT_PATH must be defined for all options other than create (for which the path is an explicit parameter)

