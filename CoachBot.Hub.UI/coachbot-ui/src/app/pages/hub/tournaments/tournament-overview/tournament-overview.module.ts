import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { SpinnerModule } from 'src/app/core/components/spinner/spinner.module';
import { RouterModule } from '@angular/router';
import { NgxPaginationModule } from 'ngx-pagination';
import { SweetAlert2Module } from '@sweetalert2/ngx-sweetalert2';
import { TournamentOverviewComponent } from './tournament-overview.component';
import { TournamentOverviewStandingsComponent } from './tournament-overview-standings/tournament-overview-standings.component';
import { TournamentOverviewFixturesComponent } from './tournament-overview-fixtures/tournament-overview-fixtures.component';
import { TournamentOverviewPlayersComponent } from './tournament-overview-players/tournament-overview-players.component';
import { TournamentOverviewTeamsComponent } from './tournament-overview-teams/tournament-overview-teams.component';
import { TournamentOverviewStaffComponent } from './tournament-overview-staff/tournament-overview-staff.component';
import { TournamentOverviewRoutingModule } from './tournament-overview.routing-module';
import { TeamListModule } from '../../team-list/team-list.module';
import { PlayerListModule } from '../../player-list/player-list.module';
import { RecentMatchesModule } from '../../recent-matches/recent-matches.module';
import { HubPipesModule } from '../../shared/pipes/hub-pipes.module';

@NgModule({
    declarations: [
        TournamentOverviewComponent,
        TournamentOverviewStandingsComponent,
        TournamentOverviewFixturesComponent,
        TournamentOverviewPlayersComponent,
        TournamentOverviewTeamsComponent,
        TournamentOverviewStaffComponent
    ],
    imports: [
        CommonModule,
        RouterModule,
        FormsModule,
        SpinnerModule,
        NgxPaginationModule,
        SweetAlert2Module,
        TournamentOverviewRoutingModule,
        TeamListModule,
        PlayerListModule,
        RecentMatchesModule,
        HubPipesModule
    ]
})
export class TournamentOverviewModule { }
