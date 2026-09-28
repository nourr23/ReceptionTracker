# ReceptionTracker

Application web permettant au magasinier d'un magasin de sport de **consulter une commande fournisseur en attente et d'en valider la réception** à trois niveaux : palette, carton ou produit.

- **Back-end** : .NET 10 (C#), API REST minimale, architecture en couches, EF Core + SQLite
- **Front-end** : React 19 + TypeScript, Vite, TanStack Query
- **Tests** : 31 tests .NET (domaine + intégration HTTP) et 24 tests front (Vitest + Testing Library + MSW)

---

## Sommaire

1. [Lancer le projet](#1-lancer-le-projet)
2. [Lancer les tests](#2-lancer-les-tests)
3. [Architecture](#3-architecture)
4. [Choix techniques](#4-choix-techniques)
5. [Zones de flou et hypothèses](#5-zones-de-flou-et-hypothèses)
6. [Pistes d'amélioration](#6-pistes-damélioration)

---

## 1. Lancer le projet

### Prérequis

| Outil       | Version                     |
| ----------- | --------------------------- |
| .NET SDK    | 10.0                        |
| Node.js     | 22.12+ (ou 20.19+)          |

Aucune base de données à installer : SQLite est un simple fichier, créé et rempli automatiquement au premier démarrage.

### Démarrage (deux terminaux, depuis la racine du dépôt)

**Terminal 1 – API**

```bash
dotnet run --project src/ReceptionTracker.Api
```

Au premier lancement, l'API crée `reception.db`, applique la migration EF Core et insère deux commandes factices (`CMD-2026`, `CMD-2027`).

**Terminal 2 – Front-end**

```bash
cd web
npm install
npm run dev
```

### Adresses

| URL                                  | Contenu                                     |
| ------------------------------------ | ------------------------------------------- |
| http://localhost:5173                | Application (front-end)                     |
| http://localhost:5143/scalar         | Documentation interactive de l'API          |
| http://localhost:5143/openapi/v1.json | Contrat OpenAPI                            |

Le fichier [`ReceptionTracker.Api.http`](src/ReceptionTracker.Api/ReceptionTracker.Api.http) contient toutes les requêtes, prêtes à être envoyées depuis Rider ou VS Code (extension REST Client).

### Réinitialiser les données

Arrêter l'API, supprimer `src/ReceptionTracker.Api/reception.db`, puis relancer : la base est recréée et ré-alimentée.

---

## 2. Lancer les tests

```bash
dotnet test            # depuis la racine : 12 tests domaine + 19 tests d'intégration
```

```bash
cd web
npm test               # 24 tests front
npm run lint           # ESLint (règles typées)
npm run typecheck      # TypeScript strict
```

| Projet                          | Type        | Ce qui est vérifié                                                                                                   |
| ------------------------------- | ----------- | -------------------------------------------------------------------------------------------------------------------- |
| `ReceptionTracker.Domain.Tests` | Unitaire    | Les règles métier seules : statuts dérivés, validation en cascade, décochage, calcul de l'avancement, invariants    |
| `ReceptionTracker.Api.Tests`    | Intégration | Toute la chaîne HTTP → service → EF Core → SQLite (en mémoire, isolée par test) : routes, JSON, persistance, erreurs 404/400, idempotence |
| `web` (Vitest)                  | UI          | L'application complète du point de vue du magasinier, contre une fausse API (MSW) qui respecte le contrat          |

---

## 3. Architecture

### Vue d'ensemble

```
ReceptionTracker/
├── src/
│   ├── ReceptionTracker.Domain/          Modèle métier et règles (aucune dépendance)
│   ├── ReceptionTracker.Application/     Cas d'usage, DTO, abstractions (IOrderRepository…)
│   ├── ReceptionTracker.Infrastructure/  EF Core, SQLite, migrations, données factices
│   └── ReceptionTracker.Api/             Endpoints HTTP, erreurs, OpenAPI
├── tests/
│   ├── ReceptionTracker.Domain.Tests/
│   └── ReceptionTracker.Api.Tests/
└── web/                                  Front-end React + TypeScript
```

### Back-end : architecture en couches (« Clean Architecture »)

```
        ┌──────────────┐
        │     Api      │  HTTP : routes, JSON, codes de statut
        └──┬────────┬──┘
           │        │
           ▼        ▼
┌──────────────┐  ┌────────────────┐
│ Application  │◄─│ Infrastructure │  EF Core implémente les interfaces
└──────┬───────┘  └────────────────┘  définies par Application
       ▼
┌──────────────┐
│    Domain    │  Order → Pallet → Carton → ProductLine
└──────────────┘
```

Les dépendances pointent toujours **vers le domaine**. Les règles métier ne dépendent ni d'EF Core ni d'ASP.NET : elles se testent en C# pur, et la base de données pourrait être remplacée en ne modifiant que l'Infrastructure.

**Parcours d'une validation** (`PUT /api/orders/CMD-2026/pallets/PAL-01/cartons/CART-01-A/reception`) :

1. **Api** : lit la route et le corps `{ "isReceived": true }`, appelle le service.
2. **Application** : charge la commande via `IOrderRepository`, trouve le carton, appelle `carton.SetReceived(true)`, enregistre via `IUnitOfWork`, renvoie la commande rafraîchie sous forme de DTO.
3. **Domain** : coche chaque produit du carton. Les statuts du carton, de la palette et de la commande se recalculent d'eux-mêmes.
4. **Api** : traduit le `Result` en `200 OK`, ou en `404` au format ProblemDetails.

### Front-end : organisation par fonctionnalité

```
web/src/
├── api/                 Client HTTP typé, gestion des erreurs, types générés depuis OpenAPI
├── components/          Composants génériques (case à cocher trois états, alerte…)
├── features/reception/  Requêtes, règles d'affichage, arbre de la commande
├── hooks/               Hooks génériques
└── test/                Fausse API (MSW), configuration des tests
```

---

## 4. Choix techniques

### Le choix central : seul le produit stocke un état

`ProductLine.IsReceived` est **la seule donnée enregistrée**. Les statuts des cartons, des palettes et de la commande (`Pending`, `PartiallyReceived`, `Received`) sont **calculés à chaque lecture** à partir des produits.

- La règle de gestion du sujet (« si je valide tous les produits un par un, le carton parent doit s'afficher comme validé ; si je décoche un produit, le carton et la palette repassent en partiellement reçu ») **est vraie par construction** : il n'existe aucun indicateur parent qui pourrait se désynchroniser.
- Valider une palette ou un carton revient simplement à cocher tous les produits en dessous.
- Le front n'implémente **aucune** règle métier : il affiche les statuts calculés par le serveur.

### Back-end

| Choix | Justification |
| --- | --- |
| **Architecture en couches** + projets séparés | Séparation nette des responsabilités, règles métier testables isolément, dépendances vérifiées par le compilateur. |
| **Modèle riche (DDD léger)** : setters privés, constructeurs validants, `Order` comme racine d'agrégat | Impossible de créer un état invalide (quantité ≤ 0, code vide, palette en double) ou de contourner les règles. |
| **SQLite + EF Core** plutôt qu'In-Memory | Une vraie base SQL, sans installation. Migrations, clés étrangères, index uniques et contrainte `CHECK (ExpectedQuantity > 0)` : les données sont protégées aussi côté base. |
| Mapping EF en **configuration Fluent** (hors du domaine) et **clés étrangères « shadow »** | Le domaine ne connaît pas EF Core. |
| **Pattern `Result<T>`** pour les erreurs attendues | « Commande introuvable » n'est pas un bug : pas d'exception, le résultat est traduit en `404`. Les exceptions restent réservées aux vrais problèmes (→ `500` sans détail technique). |
| Erreurs au format **ProblemDetails (RFC 9457)** avec un `code` métier (`Carton.NotFound`…) | Format standard, et le client peut réagir à une erreur précise. |
| **DTO** distincts des entités | Contrat JSON découplé du modèle de base ; il reprend la structure du sujet (`orderId`, `pallets`, `ref`…) et ajoute statut + avancement à chaque niveau. |
| **PUT avec valeur explicite** (`{ "isReceived": true }`) plutôt qu'un « toggle » | Idempotent : un double-clic ou un renvoi réseau ne défait pas l'action. `isReceived` est `required` : un corps vide renvoie `400` au lieu de tout décocher silencieusement. |
| Chaque mise à jour **renvoie la commande complète** | Un clic change le produit, son carton, sa palette et l'avancement : le front reçoit tout en un seul aller-retour. |
| **Minimal APIs** + `TypedResults` + **OpenAPI/Scalar** | Endpoints courts et lisibles ; réponses documentées automatiquement. |
| **Central Package Management**, `Directory.Build.props`, avertissements traités comme erreurs | Versions NuGet et paramètres définis une seule fois pour toute la solution. |

### Front-end

| Choix | Justification |
| --- | --- |
| **Types TypeScript générés depuis OpenAPI** (`openapi-typescript` + `openapi-fetch`) | L'API est la seule source de vérité : si un DTO C# change, le front ne compile plus au lieu de casser à l'exécution. |
| **TanStack Query** | Gestion standard de l'état serveur : cache, chargement, erreurs, annulation des requêtes, rafraîchissement. |
| Mises à jour d'une même commande **exécutées l'une après l'autre** (`scope` de mutation) | Une réponse lente ne peut pas arriver en dernier et écraser un état plus récent. |
| **Retour visuel immédiat** | La case cliquée affiche sa nouvelle valeur pendant la requête (et se désactive), puis la réponse du serveur fait foi. |
| **Interface non surchargée** | Palettes dépliées, cartons repliés : les produits n'apparaissent qu'à la demande. Chaque niveau affiche un compteur « x / y » et un statut. Option « Masquer les éléments reçus » pour ne voir que le reste à faire. |
| **Case à cocher à trois états** native (`indeterminate`) | Les lecteurs d'écran l'annoncent « mixte » ; toute la ligne est cliquable (confortable sur une tablette d'entrepôt). |
| **Accessibilité** | `aria-expanded`, `role="progressbar"`, `role="alert"`, focus clavier visible, respect de `prefers-reduced-motion`. |
| **Proxy Vite** vers l'API | Le front appelle `/api` sur sa propre origine : pas de CORS, mêmes URLs qu'en production derrière un reverse proxy. |
| **CSS Modules** + variables CSS | Pas de dépendance de style, thème clair/sombre automatique, mise en page responsive (téléphone / tablette). |
| **Commande sélectionnée dans l'URL** (`?order=CMD-2026`) | Survit au rechargement, partageable, compatible avec les boutons précédent/suivant. |

---

## 5. Zones de flou et hypothèses

Le sujet laisse plusieurs points ouverts. Voici les choix retenus et leurs raisons.

| Question | Hypothèse retenue | Pourquoi |
| --- | --- | --- |
| **Que signifie « valider » un produit ?** | Une case à cocher : la **quantité attendue est entièrement reçue**. Pas de saisie de quantité réellement reçue. | Le sujet parle de cocher / décocher et d'états indéterminés, ce qui correspond à un booléen. La saisie de quantités partielles (écarts de livraison) est une évolution naturelle (voir §6). |
| **« X / Y articles reçus » : articles ou lignes ?** | **Articles = unités** (somme des quantités attendues : « 60 / 255 articles reçus »). Le nombre de **lignes produit** est affiché à côté (« 2 / 9 lignes »). | « Article » désigne plutôt une unité physique ; les deux indicateurs sont utiles au magasinier. |
| **Cliquer sur un parent partiellement reçu** | Il est **entièrement validé** (tous ses produits sont cochés). Cliquer sur un parent déjà reçu le décoche entièrement. | Comportement standard d'une case à trois états. |
| **Commande vs Livraison** | Une commande = une livraison (`orderId` identifie la livraison, comme dans l'exemple de payload). | L'exemple du sujet ne distingue pas les deux. |
| **Identifiants dans les URLs** | Codes métier pour commande / palette / carton (`CMD-2026`, `PAL-01`, `CART-01-A`) ; **identifiant technique** pour les produits. | Une même référence (`TSH-RED-M`) peut apparaître dans plusieurs cartons : ce n'est pas un identifiant unique. |
| **Unicité des codes** | Palette unique dans sa commande, carton unique **dans sa palette** → route imbriquée `/pallets/{palletId}/cartons/{cartonId}`. | Hypothèse la moins restrictive, garantie par des index uniques en base. |
| **« La commande en cours »** | Plusieurs commandes existent ; la liste permet d'en choisir une, et la première non entièrement reçue s'ouvre par défaut. | Plus réaliste qu'une seule commande codée en dur. |
| **Clôture de la réception** | Pas d'action « Clôturer » : une commande est « Reçue » quand tous ses produits sont cochés, et elle reste modifiable (correction d'erreur). | Le sujet ne décrit pas de clôture. Ce serait une évolution naturelle (voir §6). |
| **Création de commandes** | Aucune : données factices insérées au démarrage (seed EF Core). | Explicitement autorisé par le sujet. |
| **Plusieurs magasiniers sur la même commande** | Le dernier enregistrement l'emporte. | Les mises à jour posent une valeur explicite (jamais d'inversion), donc les conflits sont rares et sans gravité. Un vrai contrôle de concurrence est proposé en §6. |
| **Authentification** | Aucune. | Hors du périmètre du sujet. |

---

## 6. Pistes d'amélioration

- **Quantités reçues** : saisir la quantité réellement reçue par produit et signaler les écarts (manquants, surplus).
- **Clôture de la réception** : action « Terminer la réception » qui fige la commande et la retire de la liste des commandes en attente.
- **Concurrence optimiste** : version de ligne / ETag, réponse `409 Conflict` si la commande a changé entre-temps.
- **Authentification et traçabilité** : qui a validé quoi et quand.
- **Intégration continue** (GitHub Actions) : build, tests, lint et vérification des types à chaque push.
- **Tests de bout en bout** (Playwright) : le vrai front contre la vraie API.
- **Docker Compose** pour tout démarrer en une commande ; endpoint `/health`.
