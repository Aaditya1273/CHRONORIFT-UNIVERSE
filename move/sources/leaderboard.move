/// ChronoRift Universe - Leaderboard Module
/// Track player rankings and achievements
module chronorift::leaderboard {
    use sui::object::{Self, UID};
    use sui::tx_context::{Self, TxContext};
    use sui::transfer;
    use sui::event;
    use std::string::String;

    /// Global leaderboard
    public struct Leaderboard has key {
        id: UID,
        total_players: u64,
        total_levels_completed: u64,
        total_score: u64,
    }

    /// Player stats
    public struct PlayerStats has key {
        id: UID,
        player: address,
        total_score: u64,
        levels_completed: u64,
        nfts_owned: u64,
        cru_earned: u64,
        rank: u64,
        last_updated: u64,
    }

    /// Event for score update
    public struct ScoreUpdated has copy, drop {
        player: address,
        old_score: u64,
        new_score: u64,
        rank: u64,
        timestamp: u64,
    }

    /// Event for level completion
    public struct LevelCompletedEvent has copy, drop {
        player: address,
        level_id: u64,
        score: u64,
        timestamp: u64,
    }

    /// Initialize leaderboard
    public entry fun create_leaderboard(ctx: &mut TxContext) {
        let leaderboard = Leaderboard {
            id: object::new(ctx),
            total_players: 0,
            total_levels_completed: 0,
            total_score: 0,
        };
        transfer::share_object(leaderboard);
    }

    /// Create player stats
    public entry fun create_player_stats(ctx: &mut TxContext) {
        let stats = PlayerStats {
            id: object::new(ctx),
            player: tx_context::sender(ctx),
            total_score: 0,
            levels_completed: 0,
            nfts_owned: 0,
            cru_earned: 0,
            rank: 0,
            last_updated: tx_context::epoch(ctx),
        };
        transfer::transfer(stats, tx_context::sender(ctx));
    }

    /// Update player score
    public entry fun update_score(
        leaderboard: &mut Leaderboard,
        stats: &mut PlayerStats,
        score_increment: u64,
        ctx: &mut TxContext
    ) {
        let sender = tx_context::sender(ctx);
        assert!(stats.player == sender, 0);

        let old_score = stats.total_score;
        stats.total_score = stats.total_score + score_increment;
        stats.last_updated = tx_context::epoch(ctx);

        // Update global leaderboard
        leaderboard.total_score = leaderboard.total_score + score_increment;

        // Emit event
        event::emit(ScoreUpdated {
            player: sender,
            old_score,
            new_score: stats.total_score,
            rank: stats.rank,
            timestamp: tx_context::epoch(ctx),
        });
    }

    /// Record level completion
    public entry fun record_level_completion(
        leaderboard: &mut Leaderboard,
        stats: &mut PlayerStats,
        level_id: u64,
        score: u64,
        ctx: &mut TxContext
    ) {
        let sender = tx_context::sender(ctx);
        assert!(stats.player == sender, 0);

        // Update stats
        stats.levels_completed = stats.levels_completed + 1;
        stats.total_score = stats.total_score + score;
        stats.last_updated = tx_context::epoch(ctx);

        // Update leaderboard
        leaderboard.total_levels_completed = leaderboard.total_levels_completed + 1;
        leaderboard.total_score = leaderboard.total_score + score;

        // Emit event
        event::emit(LevelCompletedEvent {
            player: sender,
            level_id,
            score,
            timestamp: tx_context::epoch(ctx),
        });
    }

    /// Update CRU earned
    public entry fun update_cru_earned(
        stats: &mut PlayerStats,
        amount: u64,
        ctx: &mut TxContext
    ) {
        let sender = tx_context::sender(ctx);
        assert!(stats.player == sender, 0);

        stats.cru_earned = stats.cru_earned + amount;
        stats.last_updated = tx_context::epoch(ctx);
    }

    /// Update NFT count
    public entry fun update_nft_count(
        stats: &mut PlayerStats,
        count: u64,
        ctx: &mut TxContext
    ) {
        let sender = tx_context::sender(ctx);
        assert!(stats.player == sender, 0);

        stats.nfts_owned = count;
        stats.last_updated = tx_context::epoch(ctx);
    }

    /// Get player total score
    public fun get_total_score(stats: &PlayerStats): u64 {
        stats.total_score
    }

    /// Get levels completed
    public fun get_levels_completed(stats: &PlayerStats): u64 {
        stats.levels_completed
    }

    /// Get CRU earned
    public fun get_cru_earned(stats: &PlayerStats): u64 {
        stats.cru_earned
    }

    /// Get player rank
    public fun get_rank(stats: &PlayerStats): u64 {
        stats.rank
    }
}
