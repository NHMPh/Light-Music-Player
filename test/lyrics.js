 function mc(e) {
        if (mo)
            return eO("Authentication token request already in progress; reusing it."),
            mo;
        let t = (async () => {
            try {
                eO(`Requesting new JWT (${e})...`);
                let t = await new Promise( (e, t) => {
                    let i = document.createElement("iframe");
                    i.src = "https://lyrics.api.dacubeking.com/challenge",
                    i.style.position = "fixed",
                    i.style.bottom = "calc(20px + var(--ytmusic-player-bar-height))",
                    i.style.right = "20px",
                    i.style.width = "0px",
                    i.style.height = "0px",
                    i.style.border = "none",
                    i.style.zIndex = "999999",
                    document.body.appendChild(i);
                    let r = r => {
                        if (r.source === i.contentWindow)
                            switch (r.data.type) {
                            case "turnstile-token":
                                eO("Received Success Token:", r.data.token),
                                n(),
                                e(r.data.token);
                                break;
                            case "turnstile-error":
                                eP("Received Challenge Error:", r.data.error),
                                n(),
                                t(Error(`${el} Turnstile challenge error: ${r.data.error}`));
                                break;
                            case "turnstile-expired":
                                e_("Token expired. Resetting challenge."),
                                i.contentWindow.postMessage({
                                    type: "reset-turnstile"
                                }, "*");
                                break;
                            case "turnstile-timeout":
                                e_("Challenge timed out."),
                                n(),
                                t(Error(`${el} Turnstile challenge timed out.`))
                            }
                    }
                      , n = () => {
                        window.removeEventListener("message", r),
                        document.body.contains(i) && document.body.removeChild(i)
                    }
                    ;
                    window.addEventListener("message", r)
                }
                )
                  , i = await fetch(ea + "verify-turnstile", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify({
                        token: t
                    }),
                    credentials: "include"
                });
                if (!i.ok)
                    throw Error(`API verification failed: ${i.statusText}`);
                let r = (await i.json()).jwt;
                if ("string" != typeof r || !r)
                    throw Error("No JWT returned from API after verification.");
                return await chrome.storage.local.set({
                    jwtToken: r
                }),
                eO("New JWT received and stored."),
                r
            } catch (e) {
                return eP("Authentication process failed:", e),
                null
            }
        }
        )();
        return mo = t,
        t.finally( () => {
            mo === t && (mo = null)
        }
        ),
        t
    }