/// ChronoRift Universe - Level Completion NFT Module
/// Mints NFTs when players complete levels
module chronorift::level_nft {
    use sui::object::{Self, UID};
    use sui::tx_context::{Self, TxContext};
    use sui::transfer;
    use sui::event;
    use std::string::{Self, String};
    use std::vector;

    /// Level completion NFT
    public struct LevelNFT has key, store {
        id: UID,
        player: address,
        level_id: u64,
        world_name: String,
        completion_time: u64,
        score: u64,
        metadata_uri: String,
        minted_at: u64,
    }

    /// Player's level completion tracker
    public struct PlayerProgress has key {
        id: UID,
        player: address,
        completed_levels: vector<u64>,
        total_score: u64,
        nfts_minted: u64,
    }

    /// Event emitted when level is completed
    public struct LevelCompleted has copy, drop {
        player: address,
        level_id: u64,
        world_name: String,
        score: u64,
        nft_id: address,
        timestamp: u64,
    }

    /// Create player progress tracker
    public entry fun create_player_progress(ctx: &mut TxContext) {
        let progress = PlayerProgress {
            id: object::new(ctx),
            player: tx_context::sender(ctx),
            completed_levels: vector::empty(),
            total_score: 0,
            nfts_minted: 0,
        };
        transfer::transfer(progress, tx_context::sender(ctx));
    }

    /// Mint NFT for level completion
    public entry fun mint_level_nft(
        progress: &mut PlayerProgress,
        level_id: u64,
        world_name: vector<u8>,
        score: u64,
        metadata_uri: vector<u8>,
        ctx: &mut TxContext
    ) {
        let sender = tx_context::sender(ctx);
        
        // Check if level already completed
        assert!(!vector::contains(&progress.completed_levels, &level_id), 0);

        // Create NFT
        let nft = LevelNFT {
            id: object::new(ctx),
            player: sender,
            level_id,
            world_name: string::utf8(world_name),
            completion_time: tx_context::epoch(ctx),
            score,
            metadata_uri: string::utf8(metadata_uri),
            minted_at: tx_context::epoch(ctx),
        };

        let nft_id = object::uid_to_address(&nft.id);

        // Update progress
        vector::push_back(&mut progress.completed_levels, level_id);
        progress.total_score = progress.total_score + score;
        progress.nfts_minted = progress.nfts_minted + 1;

        // Emit event
        event::emit(LevelCompleted {
            player: sender,
            level_id,
            world_name: string::utf8(world_name),
            score,
            nft_id,
            timestamp: tx_context::epoch(ctx),
        });

        // Transfer NFT to player
        transfer::transfer(nft, sender);
    }

    /// Check if player has completed a level
    public fun has_completed_level(progress: &PlayerProgress, level_id: u64): bool {
        vector::contains(&progress.completed_levels, &level_id)
    }

    /// Get player's total score
    public fun get_total_score(progress: &PlayerProgress): u64 {
        progress.total_score
    }

    /// Get number of NFTs minted
    public fun get_nfts_minted(progress: &PlayerProgress): u64 {
        progress.nfts_minted
    }

    /// Get completed levels count
    public fun get_completed_levels_count(progress: &PlayerProgress): u64 {
        vector::length(&progress.completed_levels)
    }
}
