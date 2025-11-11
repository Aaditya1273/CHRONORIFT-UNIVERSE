/// ChronoRift Universe - Staking Pool Module
/// Stake $CRU tokens for epoch enclaves and rewards
module chronorift::staking_pool {
    use sui::object::{Self, UID};
    use sui::tx_context::{Self, TxContext};
    use sui::transfer;
    use sui::coin::{Self, Coin};
    use sui::balance::{Self, Balance};
    use sui::event;
    use chronorift::cru_token::CRU;

    /// Staking pool for epoch enclaves
    public struct StakingPool has key {
        id: UID,
        total_staked: u64,
        total_rewards: Balance<CRU>,
        reward_rate: u64, // Rewards per epoch per token
        participants: u64,
    }

    /// Individual stake record
    public struct StakeRecord has key {
        id: UID,
        staker: address,
        amount: Balance<CRU>,
        staked_at: u64,
        last_claim: u64,
        total_claimed: u64,
    }

    /// Event emitted when tokens are staked
    public struct TokensStaked has copy, drop {
        staker: address,
        amount: u64,
        timestamp: u64,
    }

    /// Event emitted when rewards are claimed
    public struct RewardsClaimed has copy, drop {
        staker: address,
        amount: u64,
        timestamp: u64,
    }

    /// Event emitted when tokens are unstaked
    public struct TokensUnstaked has copy, drop {
        staker: address,
        amount: u64,
        timestamp: u64,
    }

    /// Initialize staking pool
    public entry fun create_staking_pool(
        reward_rate: u64,
        ctx: &mut TxContext
    ) {
        let pool = StakingPool {
            id: object::new(ctx),
            total_staked: 0,
            total_rewards: balance::zero(),
            reward_rate,
            participants: 0,
        };
        transfer::share_object(pool);
    }

    /// Stake CRU tokens
    public entry fun stake_tokens(
        pool: &mut StakingPool,
        tokens: Coin<CRU>,
        ctx: &mut TxContext
    ) {
        let amount = coin::value(&tokens);
        let sender = tx_context::sender(ctx);
        let current_epoch = tx_context::epoch(ctx);

        // Create stake record
        let stake = StakeRecord {
            id: object::new(ctx),
            staker: sender,
            amount: coin::into_balance(tokens),
            staked_at: current_epoch,
            last_claim: current_epoch,
            total_claimed: 0,
        };

        // Update pool
        pool.total_staked = pool.total_staked + amount;
        pool.participants = pool.participants + 1;

        // Emit event
        event::emit(TokensStaked {
            staker: sender,
            amount,
            timestamp: current_epoch,
        });

        // Transfer stake record to user
        transfer::transfer(stake, sender);
    }

    /// Calculate pending rewards
    public fun calculate_rewards(
        pool: &StakingPool,
        stake: &StakeRecord,
        current_epoch: u64
    ): u64 {
        let epochs_passed = current_epoch - stake.last_claim;
        let staked_amount = balance::value(&stake.amount);
        (staked_amount * pool.reward_rate * epochs_passed) / 1000000
    }

    /// Claim staking rewards
    public entry fun claim_rewards(
        pool: &mut StakingPool,
        stake: &mut StakeRecord,
        ctx: &mut TxContext
    ) {
        let sender = tx_context::sender(ctx);
        let current_epoch = tx_context::epoch(ctx);
        
        assert!(stake.staker == sender, 0);

        let rewards = calculate_rewards(pool, stake, current_epoch);
        assert!(rewards > 0, 1);
        assert!(balance::value(&pool.total_rewards) >= rewards, 2);

        // Update stake record
        stake.last_claim = current_epoch;
        stake.total_claimed = stake.total_claimed + rewards;

        // Transfer rewards
        let reward_balance = balance::split(&mut pool.total_rewards, rewards);
        let reward_coin = coin::from_balance(reward_balance, ctx);
        transfer::public_transfer(reward_coin, sender);

        // Emit event
        event::emit(RewardsClaimed {
            staker: sender,
            amount: rewards,
            timestamp: current_epoch,
        });
    }

    /// Unstake tokens
    public entry fun unstake_tokens(
        pool: &mut StakingPool,
        stake: StakeRecord,
        ctx: &mut TxContext
    ) {
        let sender = tx_context::sender(ctx);
        let current_epoch = tx_context::epoch(ctx);
        
        assert!(stake.staker == sender, 0);

        let StakeRecord {
            id,
            staker: _,
            amount,
            staked_at: _,
            last_claim: _,
            total_claimed: _,
        } = stake;

        let staked_amount = balance::value(&amount);

        // Update pool
        pool.total_staked = pool.total_staked - staked_amount;
        pool.participants = pool.participants - 1;

        // Return tokens
        let tokens = coin::from_balance(amount, ctx);
        transfer::public_transfer(tokens, sender);

        // Emit event
        event::emit(TokensUnstaked {
            staker: sender,
            amount: staked_amount,
            timestamp: current_epoch,
        });

        object::delete(id);
    }

    /// Add rewards to pool (admin function)
    public entry fun add_rewards(
        pool: &mut StakingPool,
        rewards: Coin<CRU>,
    ) {
        let reward_balance = coin::into_balance(rewards);
        balance::join(&mut pool.total_rewards, reward_balance);
    }

    /// Get total staked amount
    public fun get_total_staked(pool: &StakingPool): u64 {
        pool.total_staked
    }

    /// Get number of participants
    public fun get_participants(pool: &StakingPool): u64 {
        pool.participants
    }

    /// Get stake amount
    public fun get_stake_amount(stake: &StakeRecord): u64 {
        balance::value(&stake.amount)
    }
}
