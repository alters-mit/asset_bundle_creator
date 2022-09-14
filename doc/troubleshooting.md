# Troubleshooting

*This is an incomplete list of known issues in running Asset Bundle Creator.*

- If you get this warning in the Editor log: `[warn] kq_init: detected broken kqueue; not using.: Undefined error: 0` it means that there's a problem with your Unity license. Make sure you have valid and active Unity credentials.
- On OS X, Asset Bundle Creator might crash with a `Win32Exception` in the Editor log indicating a problem with `mono-io-layer-error (5)`. If this happens, open the project in Unity Editor and Reimport All.