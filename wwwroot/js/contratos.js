(() => {
  "use strict";

  const iniciar = () => {
    const casaSelect = document.getElementById("CasaId");
    const infoCasa = document.getElementById("infoCasa");
    if (casaSelect && infoCasa && !casaSelect.dataset.infoIniciada) {
      casaSelect.dataset.infoIniciada = "true";
      casaSelect.addEventListener("change", async () => {
        if (!casaSelect.value || !infoCasa.isConnected) return;
        const resposta = await fetch(`/Casas/Informacoes/${casaSelect.value}`);
        if (!resposta.ok) return;
        const dados = await resposta.json();
        infoCasa.classList.remove("d-none");
        infoCasa.innerHTML = `<strong>${dados.nome}</strong><br>${dados.endereco}<br>Aluguel: ${Util.formatarMoeda(dados.valor)}<br>Vencimento: dia ${dados.diaVencimento}<br>Status: ${dados.status}`;
      });
    }

    const inquilinoSelect = document.getElementById("InquilinoId");
    const infoInquilino = document.getElementById("infoInquilino");
    if (inquilinoSelect && infoInquilino && !inquilinoSelect.dataset.infoIniciada) {
      inquilinoSelect.dataset.infoIniciada = "true";
      inquilinoSelect.addEventListener("change", async () => {
        if (!inquilinoSelect.value || !infoInquilino.isConnected) return;
        const resposta = await fetch(`/Inquilinos/Informacoes/${inquilinoSelect.value}`);
        if (!resposta.ok) return;
        const dados = await resposta.json();
        infoInquilino.classList.remove("d-none");
        infoInquilino.innerHTML = `<strong>${dados.nomeCompleto}</strong><br>CPF: ${dados.cpf || "-"}<br>Telefone: ${dados.telefone || "-"}<br>E-mail: ${dados.email || "-"}`;
      });
    }
  };

  document.addEventListener("app:navigated", iniciar);
  iniciar();
})();
