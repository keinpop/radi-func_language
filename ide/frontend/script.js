let editor = ace.edit("editor");
editor.setTheme("ace/theme/monokai");
editor.setOptions({
    enableBasicAutocompletion: true,
    enableLiveAutocompletion: true,
});
editor.session.setMode("ace/mode/radi");

let count = 0

const playButton  = document.getElementById('header__playButton')
playButton.addEventListener('click',function() {
    if (playButton.hasAttribute("disabled")) return;

    let consoleInput = document.getElementById('console__textarea').value;
    let editorInput = editor.getValue()
    let answer = ""
    playButton.setAttribute("disabled", true);

    fetch("http://localhost:8080", {
        method: "POST",
        body: JSON.stringify({
            consoleInput,
            editorInput,
        }),
    })
    .then((resp) => {
        if (!resp.ok) {
            throw new Error("Failed with HTTP code " + resp.status);
        }
        return resp.json();
    })
    .then(respJSON => {
        answer += respJSON.message
        document.getElementById('console__textarea').value = answer
    })
    .catch((err) => {
        console.error(err)
    })
    .finally(() => {
        playButton.removeAttribute("disabled");
    })

},false);

