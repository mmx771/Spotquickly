function getAccessTokenFromUrl() {
    const params = new URLSearchParams(window.location.search);
    return {
        token: params.get("token"),
        refreshToken: params.get("refresh_token")
    };
}

const { token, refreshToken } = getAccessTokenFromUrl();

if (token) {
    // Top Tracks
    fetch(`https://spotquickly.onrender.com/api/auth/top-tracks?token=${token}`)
    .then(res => res.json())
    .then(data => {
            const list = document.getElementById("track-list");
            if (data && data.length > 0) {
                data.forEach(track => {
                    const li = document.createElement("li");
                    li.classList.add("track-info");
                    li.innerHTML = `
                        <span>${track.name} - ${track.artists[0].name}</span>
                        <a href="${track.album.external_urls.spotify}" target="_blank">Ver en Spotify</a>
                    `;
                    list.appendChild(li);
                });
            } else {
                const li = document.createElement("li");
                li.textContent = "No se encontraron canciones.";
                list.appendChild(li);
            }
        });

    // Playlists
    fetch(`https://spotquickly.onrender.com/api/auth/playlists?token=${token}`)
    .then(res => res.json())
    .then(data => {
            const playlistList = document.getElementById("playlist-list");
            data.forEach(playlist => {
                const li = document.createElement("li");
                li.classList.add("playlist-info");
                li.innerHTML = `
                    <span>${playlist.name}</span>
                    <a href="${playlist.external_urls.spotify}" target="_blank">Ver en Spotify</a>
                `;
                li.onclick = () => {
                    loadPlaylistTracks(playlist.id, playlist.name);
                };
                playlistList.appendChild(li);
            });
        });
} else {
    const list = document.getElementById("track-list");
    const li = document.createElement("li");
    li.textContent = "Inicia sesión con Spotify para ver tus canciones más escuchadas.";
    list.appendChild(li);
}

function loadPlaylistTracks(playlistId, playlistName) {
    document.getElementById("playlist-title").textContent = `Canciones de: ${playlistName}`;
    document.getElementById("playlist-title").style.display = "block";

    fetch(`https://spotquickly.onrender.com/api/auth/playlist-tracks?token=${token}&playlistId=${playlistId}`)
    .then(res => res.json())
    .then(data => {
            const list = document.getElementById("playlist-tracks");
            list.innerHTML = "";
            if (data && data.length > 0) {
                data.forEach(track => {
                    const li = document.createElement("li");
                    li.classList.add("track-info");
                    li.innerHTML = `
                        <span>${track.name} - ${track.artists[0].name}</span>
                        <a href="${track.album.external_urls.spotify}" target="_blank">Ver en Spotify</a>
                    `;
                    list.appendChild(li);
                });
            } else {
                const li = document.createElement("li");
                li.textContent = "Esta playlist está vacía.";
                list.appendChild(li);
            }
        });
}
