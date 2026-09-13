(() => {
  const corpo = document.body;
  const botaoSidebar = document.getElementById("botaoSidebar");
  const backdrop = document.getElementById("sidebarBackdrop");

  const telaPequena = () => window.matchMedia("(max-width: 991.98px)").matches;

  if (!telaPequena() && localStorage.getItem("sidebarRecolhida") === "1") {
    corpo.classList.add("sidebar-collapsed");
  }

  const alternarSidebar = () => {
    if (telaPequena()) {
      corpo.classList.toggle("sidebar-open");
      return;
    }

    corpo.classList.toggle("sidebar-collapsed");
    localStorage.setItem("sidebarRecolhida", corpo.classList.contains("sidebar-collapsed") ? "1" : "0");
  };

  botaoSidebar?.addEventListener("click", alternarSidebar);
  backdrop?.addEventListener("click", () => corpo.classList.remove("sidebar-open"));

  window.addEventListener("resize", () => {
    if (telaPequena()) {
      corpo.classList.remove("sidebar-collapsed");
    } else {
      corpo.classList.remove("sidebar-open");
    }
  });

  const aplicarMascara = (entrada, formatador) => {
    entrada.addEventListener("input", () => {
      const posicao = entrada.selectionStart;
      const anterior = entrada.value;
      entrada.value = formatador(entrada.value);
      const delta = entrada.value.length - anterior.length;
      entrada.setSelectionRange(Math.max(0, (posicao ?? 0) + delta), Math.max(0, (posicao ?? 0) + delta));
    });
  };

  const soDigitos = (valor) => valor.replace(/\D/g, "");

  document.querySelectorAll("[data-mascara='cpf']").forEach((el) => {
    aplicarMascara(el, (v) => {
      const d = soDigitos(v).slice(0, 11);
      return d
        .replace(/(\d{3})(\d)/, "$1.$2")
        .replace(/(\d{3})(\d)/, "$1.$2")
        .replace(/(\d{3})(\d{1,2})$/, "$1-$2");
    });
  });

  document.querySelectorAll("[data-mascara='telefone']").forEach((el) => {
    aplicarMascara(el, (v) => {
      const d = soDigitos(v).slice(0, 11);
      if (d.length <= 10) {
        return d.replace(/(\d{2})(\d)/, "($1) $2").replace(/(\d{4})(\d)/, "$1-$2");
      }
      return d.replace(/(\d{2})(\d)/, "($1) $2").replace(/(\d{5})(\d)/, "$1-$2");
    });
  });

  document.querySelectorAll("[data-mascara='cep']").forEach((el) => {
    aplicarMascara(el, (v) => soDigitos(v).slice(0, 8).replace(/(\d{5})(\d)/, "$1-$2"));
  });

  document.querySelectorAll("[data-mascara='moeda']").forEach((el) => {
    el.addEventListener("blur", () => {
      const n = Number(String(el.value).replace(/\./g, "").replace(",", "."));
      if (!Number.isNaN(n) && el.value !== "") {
        el.value = n.toLocaleString("pt-BR", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
      }
    });
  });

  const modalExclusao = document.getElementById("modalExclusao");
  if (modalExclusao) {
    modalExclusao.addEventListener("show.bs.modal", (evento) => {
      const botao = evento.relatedTarget;
      const form = modalExclusao.querySelector("form");
      const mensagem = modalExclusao.querySelector("[data-mensagem-exclusao]");
      if (botao && form) {
        form.action = botao.getAttribute("data-url") || form.action;
        if (mensagem) {
          mensagem.textContent = botao.getAttribute("data-mensagem") || "Tem certeza que deseja excluir este registro?";
        }
      }
    });
  }

  const casaSelect = document.getElementById("CasaId");
  const infoCasa = document.getElementById("infoCasa");
  casaSelect?.addEventListener("change", async () => {
    if (!casaSelect.value || !infoCasa) return;
    const resposta = await fetch(`/Casas/Informacoes/${casaSelect.value}`);
    if (!resposta.ok) return;
    const dados = await resposta.json();
    infoCasa.classList.remove("d-none");
    infoCasa.innerHTML = `<strong>${dados.nome}</strong><br>${dados.endereco}<br>Aluguel: ${Number(dados.valor).toLocaleString("pt-BR", { style: "currency", currency: "BRL" })}<br>Vencimento: dia ${dados.diaVencimento}<br>Status: ${dados.status}`;
  });

  const inquilinoSelect = document.getElementById("InquilinoId");
  const infoInquilino = document.getElementById("infoInquilino");
  inquilinoSelect?.addEventListener("change", async () => {
    if (!inquilinoSelect.value || !infoInquilino) return;
    const resposta = await fetch(`/Inquilinos/Informacoes/${inquilinoSelect.value}`);
    if (!resposta.ok) return;
    const dados = await resposta.json();
    infoInquilino.classList.remove("d-none");
    infoInquilino.innerHTML = `<strong>${dados.nomeCompleto}</strong><br>CPF: ${dados.cpf || "-"}<br>Telefone: ${dados.telefone || "-"}<br>E-mail: ${dados.email || "-"}`;
  });

  const leituraAnterior = document.querySelector("[name='LeituraAnterior']");
  const leituraAtual = document.querySelector("[name='LeituraAtual']");
  const consumo = document.querySelector("[name='Consumo']");
  const calcularConsumo = () => {
    if (!leituraAnterior || !leituraAtual || !consumo) return;
    const a = Number(leituraAnterior.value.replace(",", "."));
    const b = Number(leituraAtual.value.replace(",", "."));
    if (!Number.isNaN(a) && !Number.isNaN(b)) {
      consumo.value = (b - a).toFixed(3).replace(".", ",");
    }
  };
  leituraAnterior?.addEventListener("input", calcularConsumo);
  leituraAtual?.addEventListener("input", calcularConsumo);
})();
