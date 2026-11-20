import { Routes } from '@angular/router';
import { authGuard, adminGuard, guestGuard } from './core/guards/auth.guard';
import { Shell } from './layout/shell/shell';

export const routes: Routes = [
  {
    // Home pubblica: raggiungibile solo da chi non ha ancora una
    // sessione. Deve stare prima del blocco Shell qui sotto — condividono lo
    // stesso path '', e il router prova le rotte sorelle nell'ordine in cui
    // compaiono: la prima il cui guard passa vince.
    path: '',
    pathMatch: 'full',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/pubblico/home-pubblica/home-pubblica').then((m) => m.HomePubblica)
  },
  {
    // Pagina pubblica di utilità: nessun dato utente, accessibile a
    // chiunque, autenticato o no.
    path: 'come-funziona',
    loadComponent: () =>
      import('./features/pubblico/come-funziona/come-funziona').then((m) => m.ComeFunziona)
  },
  {
    // Informativa privacy e termini. Deve restare pubblica: e' il documento
    // che la registrazione chiede di accettare, quindi va potuto leggere
    // prima di avere un account.
    path: 'privacy',
    loadComponent: () => import('./features/pubblico/privacy/privacy').then((m) => m.Privacy)
  },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login').then((m) => m.Login)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register').then((m) => m.Register)
  },
  {
    // Vetrina del design system: strumento di sviluppo, tenuto fuori dalla shell
    // autenticata perche' non dipende da dati utente.
    path: 'design-system',
    loadComponent: () =>
      import('./features/design-system/design-system').then((m) => m.DesignSystem)
  },
  {
    path: '',
    component: Shell,
    canActivate: [authGuard],
    children: [
      {
        // Non c'e' una dashboard propria: l'ingresso dell'app autenticata e'
        // direttamente la mappa, la schermata chiave del prodotto. Un
        // utente autenticato che arriva su '/' viene qui rediretto dal
        // guestGuard della rotta pubblica sopra.
        path: '',
        pathMatch: 'full',
        redirectTo: 'mappa'
      },
      {
        path: 'la-mia-libreria',
        loadComponent: () => import('./features/libreria/libreria-list/libreria-list').then((m) => m.LibreriaList)
      },
      {
        path: 'catalogo',
        pathMatch: 'full',
        loadComponent: () => import('./features/catalogo/catalogo-list/catalogo-list').then((m) => m.CatalogoList)
      },
      {
        path: 'catalogo/:id',
        loadComponent: () =>
          import('./features/catalogo/catalogo-detail/catalogo-detail').then((m) => m.CatalogoDetail)
      },
      {
        // La mappa e' l'unica schermata che occupa tutta l'area disponibile e
        // gestisce lo scroll al proprio interno.
        path: 'mappa',
        data: { pienaLarghezza: true },
        loadComponent: () => import('./features/mappa/mappa').then((m) => m.Mappa)
      },
      {
        // Il percorso storico resta valido: il design rinomina la sezione in
        // 'Mappa', ma i collegamenti gia' condivisi non devono rompersi.
        path: 'ricerca',
        redirectTo: 'mappa'
      },
      {
        // Come la mappa, la chat gestisce da sola tre colonne a piena
        // larghezza e lo scroll dei propri pannelli. La conversazione
        // selezionata vive nel query param `p` (non in un path param) apposta:
        // due path diversi per lo stesso componente farebbero si' che il
        // router lo distrugga e ricrei a ogni selezione (perdendo elenco e
        // stato), mentre un query param sulla stessa rotta garantisce che
        // l'istanza resti la stessa.
        path: 'chat',
        data: { pienaLarghezza: true },
        loadComponent: () => import('./features/chat/chat-shell/chat-shell').then((m) => m.ChatShell)
      },
      {
        path: 'hub',
        loadComponent: () => import('./features/hub/hub-list/hub-list').then((m) => m.HubList)
      },
      {
        path: 'prestiti',
        loadComponent: () => import('./features/prestiti/prestiti-list/prestiti-list').then((m) => m.PrestitiList)
      },
      {
        // Il modello dati e' una sottoscrizione di disponibilita'
        // (NotificaService), ma la schermata e' la lista dei desideri del
        // design: lo stesso elenco letto dai due lati.
        path: 'desideri',
        loadComponent: () => import('./features/notifiche/notifiche-list/notifiche-list').then((m) => m.NotificheList)
      },
      {
        path: 'notifiche',
        redirectTo: 'desideri'
      },
      {
        path: 'profilo',
        loadComponent: () => import('./features/profilo/profilo').then((m) => m.Profilo)
      },
      {
        // Profilo pubblico di un altro utente: "pubblico" nel senso di
        // "visibile agli altri utenti", non raggiungibile senza login — resta
        // dentro la shell autenticata, non nel blocco delle rotte pubbliche.
        path: 'profilo/:userId',
        loadComponent: () =>
          import('./features/profilo-pubblico/profilo-pubblico').then((m) => m.ProfiloPubblico)
      },
      {
        path: 'impostazioni',
        loadComponent: () => import('./features/impostazioni/impostazioni').then((m) => m.Impostazioni)
      },
      {
        path: 'admin',
        loadComponent: () => import('./features/admin/admin-dashboard/admin-dashboard').then((m) => m.AdminDashboard),
        canActivate: [adminGuard]
      }
    ]
  },
  { path: '**', redirectTo: '' }
];
