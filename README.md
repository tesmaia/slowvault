# slowvault
Store secrets with a delay on retrieval.

Use this to protect passwords from yourself. Fight addiction by using your own blocking tools, protect unblocking with a password and store said password in here.
Essentially it's just a fancy key value store. Do not use this as a password manager.

---

This project is incomplete.

---
1. Build it using dotnet 9
2. Add it to your path
3. Set the environment variable SLOWVAULT_PATH to a location of choice
4. Just use slowvault --help to look up how it works, it's gonna change a bit still.

Should have cross-platform support, support for linux uses xsel to access the clipboard which might not clear things correctly.
