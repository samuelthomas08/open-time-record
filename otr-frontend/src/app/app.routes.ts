import { Routes } from '@angular/router';

import { Dashboard } from './pages/dashboard/dashboard';
import { TimeEntries } from './pages/time-entries/time-entries';
import { Login } from './pages/login/login';
import { Register } from './pages/register/register';
import { Activate } from './pages/activate/activate';
import { Users } from './pages/users/users';
import { Projects } from './pages/projects/projects';
import { CorrectionRequests } from './pages/correction-requests/correction-requests';
import { SmtpSettingsPage } from './pages/smtp-settings/smtp-settings';
import { SettingsLayout } from './pages/settings/settings-layout';
import { SettingsAppearance } from './pages/settings/appearance/appearance';
import { SettingsAbout } from './pages/settings/about/about';
import { SettingsProfile } from './pages/settings/profile/profile';
import { SettingsSystem } from './pages/settings/system/system';
import { Roles } from './pages/roles/roles';
import { Teams } from './pages/teams/teams';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  { path: 'activate', component: Activate },
  { path: '', component: Dashboard, canActivate: [authGuard] },
  { path: 'time-entries', component: TimeEntries, canActivate: [authGuard] },
  { path: 'correction-requests', component: CorrectionRequests, canActivate: [authGuard] },
  {
    path: 'settings',
    component: SettingsLayout,
    canActivate: [authGuard],
    children: [
      { path: '', component: SettingsAppearance },
      { path: 'profile', component: SettingsProfile },
      { path: 'about', component: SettingsAbout },
      { path: 'users', component: Users },
      { path: 'roles', component: Roles },
      { path: 'teams', component: Teams },
      { path: 'projects', component: Projects },
      { path: 'smtp', component: SmtpSettingsPage },
      { path: 'system', component: SettingsSystem },
    ],
  },
];
