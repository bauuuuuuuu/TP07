let publicacionesCargadas = 10;

function cargarMasPublicaciones() {
    const btn = document.getElementById("btnVerMas");
    btn.innerText = "Cargando...";
    btn.disabled = true;

    fetch(`/Home/CargarMasPublicaciones?desde=${publicacionesCargadas}`)
        .then(response => response.json())
        .then(data => {
            if (!data || data.length === 0) {
                btn.innerText = "No hay más publicaciones";
                btn.disabled = true;
                return;
            }

            const contenedor = document.getElementById("contenedor-publicaciones");

            data.forEach(pub => {
                let comentariosHtml = '';
                if (pub.comentarios && pub.comentarios.length > 0) {
                    pub.comentarios.forEach(com => {
                        comentariosHtml += `
                            <div class="comment-bubble">
                                <div class="d-flex justify-content-between align-items-baseline mb-1">
                                    <span class="comment-user">${com.nombreUsuario}</span>
                                    <span style="font-size: 0.7rem; color: #64748b;">${com.fechaComentario}</span>
                                </div>
                                <div class="comment-text">${com.texto}</div>
                            </div>
                        `;
                    });
                }

                const cardHtml = `
                    <div class="post-card">
                        ${pub.imagen ? `<div class="post-media-frame"><img src="${pub.imagen}" alt="Publicación"></div>` : ''}

                        <div class="post-content">
                            <div class="d-flex justify-content-between align-items-start mb-2">
                                <h3 class="post-title mb-0">${pub.titulo}</h3>
                            </div>

                            <p class="post-text mb-3">${pub.descripcion}</p>

                            <div class="mb-4">
                                <span class="post-author-chip">
                                    <span>👤 ${pub.nombreAutor ? pub.nombreAutor : "Usuario Desconocido"}</span>
                                    <span>•</span>
                                    <span>🕒 ${pub.fecha}</span>
                                </span>
                            </div>

                            <div class="d-flex align-items-center gap-3 mb-4 pb-2 border-bottom border-secondary border-opacity-25">
                                <button id="btn-like-${pub.id}" 
                                        class="btn-like-neon ${pub.yaDioLike ? 'active' : ''}" 
                                        onclick="darOQuitarLike(${pub.id})">
                                    ❤️ <span>Me Gusta</span>
                                </button>
                                <span id="likes-${pub.id}" class="fw-semibold" style="color: #cbd5e1; font-size: 0.9rem;">
                                    ${pub.cantidadLikes} Me Gusta
                                </span>
                            </div>

                            <div class="comments-wrapper">
                                <h6 class="text-white fw-bold mb-3" style="font-size: 0.85rem; letter-spacing: 0.05em; text-transform: uppercase; color: #94a3b8;">
                                    Conversación
                                </h6>

                                <div id="comentarios-${pub.id}" class="d-flex flex-column gap-2 mb-3">
                                    ${comentariosHtml}
                                </div>

                                <div class="input-group">
                                    <input type="text" id="input-comentario-${pub.id}" class="form-control input-neon" placeholder="Escribe tu respuesta..." />
                                    <button class="btn btn-glow-purple" onclick="comentar(${pub.id})">Enviar</button>
                                </div>
                            </div>
                        </div>
                    </div>
                `;

                contenedor.insertAdjacentHTML('beforeend', cardHtml);
            });

            publicacionesCargadas += data.length;
            btn.innerText = "Cargar más publicaciones";
            btn.disabled = false;
        })
        .catch(error => {
            console.error("Error al cargar más publicaciones:", error);
            btn.innerText = "Error al cargar. Reintentar";
            btn.disabled = false;
        });
}

function darOQuitarLike(idPublicacion) {
    const formData = new FormData();
    formData.append('idPublicacion', idPublicacion);

    fetch('/Home/ToggleMeGusta', {
        method: 'POST',
        body: formData
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            document.getElementById(`likes-${idPublicacion}`).innerText = `${data.cantidadLikes} Me Gusta`;

            const btn = document.getElementById(`btn-like-${idPublicacion}`);
            if (data.dioLike) {
                btn.classList.add('active');
            } else {
                btn.classList.remove('active');
            }
        } else {
            alert(data.message || "Ocurrió un error con el Me Gusta.");
        }
    })
    .catch(error => console.error('Error al procesar el Me Gusta:', error));
}

function comentar(idPublicacion) {
    const input = document.getElementById(`input-comentario-${idPublicacion}`);
    const texto = input.value.trim();

    if (!texto) {
        alert("Escribe un comentario antes de enviar.");
        return;
    }

    const formData = new FormData();
    formData.append('idPublicacion', idPublicacion);
    formData.append('texto', texto);

    fetch('/Home/AgregarComentario', {
        method: 'POST',
        body: formData
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            const contenedor = document.getElementById(`comentarios-${idPublicacion}`);

            const nuevoComentarioHtml = `
                <div class="comment-bubble">
                    <div class="d-flex justify-content-between align-items-baseline mb-1">
                        <span class="comment-user">${data.nombreUsuario}</span>
                        <span style="font-size: 0.7rem; color: #64748b;">${data.fecha}</span>
                    </div>
                    <div class="comment-text">${data.texto}</div>
                </div>
            `;

            contenedor.innerHTML += nuevoComentarioHtml;
            input.value = '';
        } else {
            alert(data.message || "No se pudo agregar el comentario.");
        }
    })
    .catch(error => console.error('Error al comentar:', error));
}