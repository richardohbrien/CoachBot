import { Routes } from '@angular/router';
import { ChannelsComponent } from './channels/channels.component';
import { ChannelComponent } from './channel/channel.component';
import { ServersComponent } from './servers/servers.component';
import { AnnouncementsComponent } from './announcements/announcements.component';
import { BotComponent } from './bot/bot.component';
import { LoginComponent } from './login/login.component';
import { DiscordCommandsComponent } from './discord-commands/discord-commands.component';
import { MatchHistoryComponent } from './match-history/match-history.component';
import { PlayerLeaderboardsComponent } from './player-leaderboards/player-leaderboards.component';

export const appRoutes: Routes = [
    {
        path: '',
        redirectTo: '/channels',
        pathMatch: 'full'
    },
    { path: 'channels', component: ChannelsComponent },
    { path: 'channel/:id', component: ChannelComponent },
    { path: 'servers', component: ServersComponent },
    { path: 'match-history', component: MatchHistoryComponent },
    { path: 'player-leaderboards', component: PlayerLeaderboardsComponent },
    { path: 'bot', component: BotComponent },
    { path: 'announcements', component: AnnouncementsComponent },
    { path: 'login', component: LoginComponent },
    { path: 'discord-commands', component: DiscordCommandsComponent }
];
