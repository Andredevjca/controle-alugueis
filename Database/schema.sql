-- Sistema de Controle de Aluguéis
-- MySQL 8+

CREATE DATABASE IF NOT EXISTS controle_alugueis
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE controle_alugueis;

SET FOREIGN_KEY_CHECKS = 0;

DROP TABLE IF EXISTS observacoes;
DROP TABLE IF EXISTS contas_consumo;
DROP TABLE IF EXISTS lancamentos_financeiros;
DROP TABLE IF EXISTS contratos;
DROP TABLE IF EXISTS casas;
DROP TABLE IF EXISTS inquilinos;
DROP TABLE IF EXISTS categorias_financeiras;
DROP TABLE IF EXISTS usuarios;
DROP TABLE IF EXISTS configuracoes;

SET FOREIGN_KEY_CHECKS = 1;

CREATE TABLE usuarios (
  id INT AUTO_INCREMENT PRIMARY KEY,
  nome VARCHAR(120) NOT NULL,
  email VARCHAR(160) NOT NULL UNIQUE,
  senha_hash VARCHAR(255) NOT NULL,
  perfil VARCHAR(40) NOT NULL DEFAULT 'Administrador',
  ativo TINYINT(1) NOT NULL DEFAULT 1,
  data_cadastro DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

CREATE TABLE configuracoes (
  id INT AUTO_INCREMENT PRIMARY KEY,
  chave VARCHAR(80) NOT NULL UNIQUE,
  valor VARCHAR(500) NULL,
  descricao VARCHAR(255) NULL
) ENGINE=InnoDB;

CREATE TABLE categorias_financeiras (
  id INT AUTO_INCREMENT PRIMARY KEY,
  nome VARCHAR(80) NOT NULL,
  tipo VARCHAR(20) NOT NULL,
  ativo TINYINT(1) NOT NULL DEFAULT 1
) ENGINE=InnoDB;

CREATE TABLE casas (
  id INT AUTO_INCREMENT PRIMARY KEY,
  nome VARCHAR(120) NOT NULL,
  cep VARCHAR(12) NULL,
  endereco VARCHAR(180) NOT NULL,
  numero VARCHAR(20) NULL,
  complemento VARCHAR(80) NULL,
  bairro VARCHAR(80) NULL,
  cidade VARCHAR(80) NOT NULL,
  estado CHAR(2) NOT NULL,
  valor_aluguel DECIMAL(12,2) NOT NULL DEFAULT 0,
  dia_vencimento INT NOT NULL DEFAULT 10,
  area_m2 DECIMAL(10,2) NULL,
  qtd_quartos INT NOT NULL DEFAULT 0,
  qtd_banheiros INT NOT NULL DEFAULT 0,
  qtd_vagas INT NOT NULL DEFAULT 0,
  numero_medidor_agua VARCHAR(80) NULL,
  numero_medidor_luz VARCHAR(80) NULL,
  status VARCHAR(30) NOT NULL DEFAULT 'Disponível',
  observacoes TEXT NULL,
  ativo TINYINT(1) NOT NULL DEFAULT 1,
  data_cadastro DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

CREATE TABLE inquilinos (
  id INT AUTO_INCREMENT PRIMARY KEY,
  nome_completo VARCHAR(160) NOT NULL,
  cpf VARCHAR(14) NULL,
  rg VARCHAR(20) NULL,
  data_nascimento DATE NULL,
  telefone VARCHAR(20) NULL,
  whatsapp VARCHAR(20) NULL,
  email VARCHAR(160) NULL,
  profissao VARCHAR(80) NULL,
  renda DECIMAL(12,2) NULL,
  endereco_anterior VARCHAR(255) NULL,
  identificacao_conta_agua VARCHAR(80) NULL,
  identificacao_conta_luz VARCHAR(80) NULL,
  observacoes TEXT NULL,
  ativo TINYINT(1) NOT NULL DEFAULT 1,
  data_cadastro DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

CREATE TABLE contratos (
  id INT AUTO_INCREMENT PRIMARY KEY,
  numero VARCHAR(40) NOT NULL,
  casa_id INT NOT NULL,
  inquilino_id INT NOT NULL,
  data_inicio DATE NOT NULL,
  data_termino DATE NULL,
  data_fim DATE NULL,
  valor_aluguel DECIMAL(12,2) NOT NULL,
  dia_vencimento INT NOT NULL DEFAULT 10,
  valor_caucao DECIMAL(12,2) NOT NULL DEFAULT 0,
  meses_caucao INT NOT NULL DEFAULT 0,
  indice_reajuste VARCHAR(40) NULL,
  percentual_multa DECIMAL(8,2) NOT NULL DEFAULT 0,
  percentual_juros DECIMAL(8,2) NOT NULL DEFAULT 0,
  status VARCHAR(20) NOT NULL DEFAULT 'Ativo',
  observacoes TEXT NULL,
  data_cadastro DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT fk_contrato_casa FOREIGN KEY (casa_id) REFERENCES casas(id),
  CONSTRAINT fk_contrato_inquilino FOREIGN KEY (inquilino_id) REFERENCES inquilinos(id),
  INDEX ix_contratos_casa (casa_id),
  INDEX ix_contratos_inquilino (inquilino_id),
  INDEX ix_contratos_status (status)
) ENGINE=InnoDB;

CREATE TABLE lancamentos_financeiros (
  id INT AUTO_INCREMENT PRIMARY KEY,
  casa_id INT NULL,
  contrato_id INT NULL,
  inquilino_id INT NULL,
  tipo VARCHAR(20) NOT NULL,
  categoria_id INT NULL,
  origem VARCHAR(40) NOT NULL,
  descricao VARCHAR(255) NOT NULL,
  data_lancamento DATE NOT NULL,
  vencimento DATE NOT NULL,
  valor DECIMAL(12,2) NOT NULL,
  data_pagamento DATE NULL,
  status VARCHAR(20) NOT NULL DEFAULT 'Pendente',
  observacoes TEXT NULL,
  data_cadastro DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT fk_fin_casa FOREIGN KEY (casa_id) REFERENCES casas(id),
  CONSTRAINT fk_fin_contrato FOREIGN KEY (contrato_id) REFERENCES contratos(id),
  CONSTRAINT fk_fin_inquilino FOREIGN KEY (inquilino_id) REFERENCES inquilinos(id),
  CONSTRAINT fk_fin_categoria FOREIGN KEY (categoria_id) REFERENCES categorias_financeiras(id),
  INDEX ix_fin_vencimento (vencimento),
  INDEX ix_fin_status (status)
) ENGINE=InnoDB;

CREATE TABLE contas_consumo (
  id INT AUTO_INCREMENT PRIMARY KEY,
  casa_id INT NOT NULL,
  tipo VARCHAR(10) NOT NULL,
  referencia VARCHAR(7) NOT NULL,
  leitura_anterior DECIMAL(12,3) NULL,
  leitura_atual DECIMAL(12,3) NULL,
  consumo DECIMAL(12,3) NULL,
  valor DECIMAL(12,2) NOT NULL,
  vencimento DATE NOT NULL,
  data_pagamento DATE NULL,
  status VARCHAR(20) NOT NULL DEFAULT 'Pendente',
  observacoes TEXT NULL,
  data_cadastro DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT fk_consumo_casa FOREIGN KEY (casa_id) REFERENCES casas(id),
  INDEX ix_consumo_casa_tipo (casa_id, tipo)
) ENGINE=InnoDB;

CREATE TABLE observacoes (
  id INT AUTO_INCREMENT PRIMARY KEY,
  titulo VARCHAR(160) NOT NULL,
  descricao TEXT NOT NULL,
  tipo VARCHAR(40) NOT NULL,
  casa_id INT NULL,
  contrato_id INT NULL,
  inquilino_id INT NULL,
  lancamento_id INT NULL,
  conta_consumo_id INT NULL,
  data_observacao DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  usuario_id INT NULL,
  CONSTRAINT fk_obs_casa FOREIGN KEY (casa_id) REFERENCES casas(id),
  CONSTRAINT fk_obs_contrato FOREIGN KEY (contrato_id) REFERENCES contratos(id),
  CONSTRAINT fk_obs_inquilino FOREIGN KEY (inquilino_id) REFERENCES inquilinos(id),
  CONSTRAINT fk_obs_usuario FOREIGN KEY (usuario_id) REFERENCES usuarios(id)
) ENGINE=InnoDB;

INSERT INTO categorias_financeiras (nome, tipo) VALUES
('Aluguel', 'Receita'),
('Caução', 'Receita'),
('Multa', 'Receita'),
('Juros', 'Receita'),
('Outros', 'Receita'),
('Manutenção', 'Despesa'),
('Conserto', 'Despesa'),
('Imposto', 'Despesa'),
('Seguro', 'Despesa'),
('Taxa', 'Despesa'),
('Outros', 'Despesa');

INSERT INTO configuracoes (chave, valor, descricao) VALUES
('nome_sistema', 'Controle de Aluguéis', 'Nome exibido no sistema'),
('empresa', 'Administração Imobiliária', 'Nome da empresa'),
('moeda', 'BRL', 'Moeda padrão');

INSERT INTO casas (nome, cep, endereco, numero, bairro, cidade, estado, valor_aluguel, dia_vencimento, area_m2, qtd_quartos, qtd_banheiros, qtd_vagas, status) VALUES
('Casa 01', '01001-000', 'Rua das Palmeiras', '120', 'Centro', 'São Paulo', 'SP', 1200.00, 10, 85.00, 2, 1, 1, 'Alugada'),
('Casa 02', '01310-100', 'Avenida Paulista', '900', 'Bela Vista', 'São Paulo', 'SP', 1850.00, 5, 110.00, 3, 2, 1, 'Alugada'),
('Casa 03', '04038-001', 'Rua Domingos de Morais', '450', 'Vila Mariana', 'São Paulo', 'SP', 1500.00, 15, 92.00, 2, 2, 1, 'Alugada'),
('Casa 04', '05422-030', 'Rua Teodoro Sampaio', '210', 'Pinheiros', 'São Paulo', 'SP', 2100.00, 8, 130.00, 3, 2, 2, 'Alugada'),
('Casa 05', '02011-000', 'Rua Voluntários da Pátria', '55', 'Santana', 'São Paulo', 'SP', 980.00, 12, 70.00, 2, 1, 0, 'Disponível');

INSERT INTO inquilinos (nome_completo, cpf, rg, data_nascimento, telefone, whatsapp, email, profissao, renda, endereco_anterior) VALUES
('Carlos da Silva', '123.456.789-09', '12.345.678-9', '1988-03-12', '(11) 98888-1111', '(11) 98888-1111', 'carlos.silva@email.com', 'Analista', 4500.00, 'Rua A, 10'),
('Ana Souza', '987.654.321-00', '98.765.432-1', '1992-07-22', '(11) 97777-2222', '(11) 97777-2222', 'ana.souza@email.com', 'Designer', 5200.00, 'Rua B, 20'),
('Pedro Almeida', '111.222.333-44', '11.222.333-4', '1985-01-05', '(11) 96666-3333', '(11) 96666-3333', 'pedro.almeida@email.com', 'Professor', 3800.00, 'Rua C, 30'),
('Juliana Costa', '555.666.777-88', '55.666.777-8', '1990-11-18', '(11) 95555-4444', '(11) 95555-4444', 'juliana.costa@email.com', 'Enfermeira', 6100.00, 'Rua D, 40'),
('João Pereira', '222.333.444-55', '22.333.444-5', '1979-09-09', '(11) 94444-5555', '(11) 94444-5555', 'joao.pereira@email.com', 'Comerciante', 4000.00, 'Rua E, 50'),
('Maria Oliveira', '333.444.555-66', '33.444.555-6', '1983-04-30', '(11) 93333-6666', '(11) 93333-6666', 'maria.oliveira@email.com', 'Advogada', 7800.00, 'Rua F, 60');

INSERT INTO contratos (numero, casa_id, inquilino_id, data_inicio, data_termino, data_fim, valor_aluguel, dia_vencimento, valor_caucao, meses_caucao, indice_reajuste, percentual_multa, percentual_juros, status) VALUES
('001/2020', 1, 5, '2020-01-10', '2022-01-10', '2022-01-10', 900.00, 10, 900.00, 1, 'IGP-M', 10.00, 1.00, 'Encerrado'),
('002/2022', 1, 6, '2022-01-15', '2024-02-05', '2024-02-05', 1050.00, 10, 1050.00, 1, 'IGP-M', 10.00, 1.00, 'Encerrado'),
('003/2024', 1, 1, '2024-02-10', '2026-02-10', NULL, 1200.00, 10, 1200.00, 1, 'IGP-M', 10.00, 1.00, 'Ativo'),
('004/2025', 2, 2, '2025-03-01', '2027-03-01', NULL, 1850.00, 5, 1850.00, 1, 'IPCA', 10.00, 1.00, 'Ativo'),
('005/2025', 3, 3, '2025-01-15', '2026-01-15', NULL, 1500.00, 15, 1500.00, 1, 'IGP-M', 10.00, 1.00, 'Ativo'),
('006/2024', 4, 4, '2024-08-08', '2026-08-08', NULL, 2100.00, 8, 2100.00, 1, 'IPCA', 10.00, 1.00, 'Ativo');

INSERT INTO lancamentos_financeiros (casa_id, contrato_id, inquilino_id, tipo, categoria_id, origem, descricao, data_lancamento, vencimento, valor, data_pagamento, status) VALUES
(1, 3, 1, 'Receita', 1, 'Aluguel', 'Aluguel Casa 01 - Agosto/2026', '2026-08-01', '2026-08-10', 1200.00, '2026-08-09', 'Pago'),
(1, 3, 1, 'Receita', 1, 'Aluguel', 'Aluguel Casa 01 - Setembro/2026', '2026-09-01', '2026-09-10', 1200.00, NULL, 'Pendente'),
(2, 4, 2, 'Receita', 1, 'Aluguel', 'Aluguel Casa 02 - Agosto/2026', '2026-08-01', '2026-08-05', 1850.00, '2026-08-04', 'Pago'),
(2, 4, 2, 'Receita', 1, 'Aluguel', 'Aluguel Casa 02 - Setembro/2026', '2026-09-01', '2026-09-05', 1850.00, NULL, 'Pendente'),
(3, 5, 3, 'Receita', 1, 'Aluguel', 'Aluguel Casa 03 - Julho/2026', '2026-07-01', '2026-07-15', 1500.00, NULL, 'Atrasado'),
(3, 5, 3, 'Receita', 1, 'Aluguel', 'Aluguel Casa 03 - Agosto/2026', '2026-08-01', '2026-08-15', 1500.00, '2026-08-15', 'Pago'),
(4, 6, 4, 'Receita', 1, 'Aluguel', 'Aluguel Casa 04 - Agosto/2026', '2026-08-01', '2026-08-08', 2100.00, '2026-08-07', 'Pago'),
(1, NULL, NULL, 'Despesa', 6, 'Manutenção', 'Reparo hidráulico Casa 01', '2026-08-12', '2026-08-12', 350.00, '2026-08-12', 'Pago'),
(5, NULL, NULL, 'Despesa', 7, 'Conserto', 'Pintura externa Casa 05', '2026-08-20', '2026-08-25', 800.00, NULL, 'Pendente');

INSERT INTO contas_consumo (casa_id, tipo, referencia, leitura_anterior, leitura_atual, consumo, valor, vencimento, data_pagamento, status) VALUES
(1, 'Água', '2026-07', 120.000, 135.000, 15.000, 89.40, '2026-08-12', '2026-08-10', 'Pago'),
(1, 'Água', '2026-08', 135.000, 150.000, 15.000, 92.10, '2026-09-12', NULL, 'Pendente'),
(1, 'Luz', '2026-07', 980.000, 1120.000, 140.000, 210.55, '2026-08-18', '2026-08-17', 'Pago'),
(1, 'Luz', '2026-08', 1120.000, 1255.000, 135.000, 198.30, '2026-09-18', NULL, 'Pendente'),
(2, 'Água', '2026-08', 200.000, 218.000, 18.000, 110.00, '2026-09-10', NULL, 'Pendente'),
(2, 'Luz', '2026-08', 1500.000, 1680.000, 180.000, 265.00, '2026-09-15', NULL, 'Pendente');

INSERT INTO observacoes (titulo, descricao, tipo, casa_id, inquilino_id, data_observacao) VALUES
('Entrega das chaves', 'Inquilino recebeu as chaves e o inventário foi assinado.', 'Contrato', 1, 1, '2024-02-10 10:00:00'),
('Vazamento no banheiro', 'Inquilino relatou vazamento. Técnico agendado.', 'Manutenção', 1, 1, '2026-08-11 14:30:00'),
('Pagamento antecipado', 'Aluguel de agosto quitado um dia antes do vencimento.', 'Financeiro', 1, 1, '2026-08-09 09:15:00');
